using MultiOpener.Entities.Misc;
using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MultiOpener.Entities.Opened.ResetTracker
{
    /// <summary>
    /// Stats Example to do:
    /// - Time in wall
    /// - Time playing (RTA)
    /// - Time needed to enter nether
    /// - Average for all splits
    /// - time played stats
    /// </summary>
    public sealed class ResetTrackerLocal : OpenedResetTrackerProcess
    {
        private readonly Stopwatch stopwatch = new();

        private string? _recordsFolder;

        public ResetTrackerLocal() : base() { }

        public override void ActivateTracker()
        {
            if (IsTracking) return;

            _recordsFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "speedrunigt", "records");
            if (App.Config.DeleteAllRecordOnActivating)
                ClearRecordsFolder();

            _source = new();
            _token = _source.Token;

            IsTracking = true;
            UpdateStatus();

            StartViewModel.Log("Activated Tracker");
            _trackerTask = Task.Run(TrackStats, _token);
            _ = Task.Run(UIUpdate, _token);
        }
        public override void DeactivateTracker()
        {
            if (_token.IsCancellationRequested || !IsTracking) return;

            _source.Cancel();
            IsTracking = false;
            TimeToUpdateStats = updateFrequencySize;

            if (_trackerTask is { IsCompleted: false })
            {
                try
                {
                    _trackerTask.Wait(_token);
                }
                catch { }
            }

            _source.Dispose();
            stopwatch.Stop();
            UpdateUIStats();
            stopwatch.Reset();
            //TODO: 0 saving session before clearing
            SessionData.Clear();
            StartViewModel.Log("Deactivated Tracker");
        }

        private async Task TrackStats()
        {
            updateFrequencySize = App.Config.UpdateResetTrackerFrequency / 1000;
            TimeToUpdateStats = updateFrequencySize;

            while (IsTracking)
            {
                UpdateUIStats();

                try
                {
                    TimeToUpdateStats -= 1;
                    await Task.Delay(TimeSpan.FromSeconds(1), _token);
                }
                catch { break; }


                if (TimeToUpdateStats == 0)
                {
                    OnTracking();
                    TimeToUpdateStats = updateFrequencySize;
                }
            }
        }

        private void OnTracking()
        {
            if (string.IsNullOrEmpty(_recordsFolder)) return;

            long lastFileOpenedRead = SessionData.LastFileDateRead;
            var records = Directory.GetFiles(_recordsFolder, "*.json", SearchOption.TopDirectoryOnly).AsSpan();
            //mozliwe uzycie Parallel z partycjonowaniem, ale czy potrzebne jezeli juz bedzie gotowy
            for (int i = 0; i < records.Length; i++)
            {
                string text = File.ReadAllText(records[i]) ?? string.Empty;
                try
                {
                    if (string.IsNullOrEmpty(text)) continue;
                    RecordData? data = JsonSerializer.Deserialize<RecordData>(text);

                    if (data == null) continue;
                    if (data.Date <= SessionData.LastFileDateRead) continue;
                    if (data.Date >= lastFileOpenedRead) lastFileOpenedRead = data.Date;
                    if (!data.Type!.Equals("random_seed") || data.DefaultGameMode != 0) continue;
                    if (data.OpenLanTime == null && data.IsCheatAllowed) continue;

                    FilterResetData(data);
                }
                catch (JsonException ex)
                {
                    StartViewModel.Log($"Error deserializing {records[i]}: {ex.Message}", ConsoleLineOption.Error);
                }
            }
            SessionData.LastFileDateRead = lastFileOpenedRead;
        }
        private void FilterResetData(RecordData data)
        {
            bool foundIronPick = false;
            if (data.FinalRTA == 0)
            {
                SessionData.WallResets += 1;
                return;
            }

            //ADVANCEMENTS
            if (data.Advancements != null && data.Advancements.Count != 0)
            {
                //IRON PICK
                if (data.Advancements.TryGetValue("minecraft:story/iron_tools", out var story) && story.IsCompleted)
                {
                    SessionData.IronPickaxeCount += 1;
                    foundIronPick = true;
                }
            }

            //STATS
            if (data.Stats != null && data.Stats.Count != 0)
            {
                string key = GetFirstKey(data.Stats)!;
                RecordStatsCategoriesData? statsData = data.Stats[key].StatsData;

                //DIAMOND PICK
                if (statsData != null && statsData.Crafted != null && statsData.Crafted.TryGetValue("minecraft:diamond_pickaxe", out _) && !foundIronPick)
                {
                    SessionData.IronPickaxeCount += 1;
                    foundIronPick = true;
                }
            }

            //MAIN SPLITS
            if (data.Timelines == null || data.Timelines.Length == 0)
            {
                SessionData.NoNetherEnterResets += 1;
                return;
            }
            for (int j = 0; j < data.Timelines?.Length; j++)
            {
                RecordTimelinesData? prev = j - 1 >= 0 ? data.Timelines[j - 1] : null;
                RecordTimelinesData? current = data.Timelines[j];
                string name = current.Name!;

                if (data.OpenLanTime != null)
                {
                    string? lan = data.OpenLanTime.ToString();
                    if (!string.IsNullOrEmpty(lan))
                    {
                        long lanTime = long.Parse(lan);
                        if (lanTime < current.RTA)
                        {
                            SessionData.SplitlessResets += 1;
                            break;
                        }
                    }
                }

                if (name.Equals("enter_fortress") && prev != null && prev.Name!.Equals("enter_nether"))
                    name = "enter_bastion";
                else if (name.Equals("enter_bastion") && prev != null && prev.Name!.Equals("enter_fortress"))
                    name = "enter_fortress";

                SessionData.UpdateSplit(name, current.IGT);
            }
        }

        private async Task UIUpdate()
        {
            stopwatch.Start();

            while (IsTracking)
            {
                UpdateUIStats();

                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(100), _token);
                }
                catch { break; }
            }
        }
        private void UpdateUIStats()
        {
            //every 100 miliseconds
            uiUpdateCount++;
            SessionData.ElapsedTimeMiliseconds = stopwatch.ElapsedMilliseconds;

            if (uiUpdateCount % 10 == 0) //every second
            {
                SessionData.UpdatePerHourStats();
            }
        }

        private void ClearRecordsFolder()
        {
            try
            {
                if (Directory.Exists(_recordsFolder))
                {
                    DirectoryInfo directoryInfo = new(_recordsFolder);

                    FileInfo[] files = directoryInfo.GetFiles();
                    int fileCount = files.Length;

                    long totalSize = 0;
                    foreach (FileInfo file in files)
                        totalSize += file.Length;

                    if (totalSize == 0 || fileCount == 0)
                        return;

                    Stopwatch stopwatch = new();
                    stopwatch.Start();

                    StartViewModel.Log($"Clearing records folder for tracking {_recordsFolder} that contains {fileCount} files.");
                    StartViewModel.Log($"Total size of files in {_recordsFolder}: {Math.Round(totalSize / (1024f * 1024f) * 100) / 100} MB.");

                    float percentage = 0.1f;
                    if (fileCount >= 20000) percentage = 0.01f;
                    else if (fileCount >= 10000) percentage = 0.05f;

                    int stepSize = (int)Math.Floor(fileCount * percentage);
                    int stepCount = stepSize;

                    //TODO: 9 zrobic popup okno z progress barem do takich rzeczy informujac z dokladnym postepem usuwania itp itd
                    for (int i = 0; i < files.Length; i++)
                    {
                        var file = files[i];
                        if (i >= stepCount)
                        {
                            stepCount += stepSize;
                            StartViewModel.Log($"Cleared [{i}/{fileCount}] - {Math.Round(((float)i / fileCount) * 100 * 100) / 100}% done");
                        }
                        file.Delete();
                    }
                    StartViewModel.Log($"Cleared [{fileCount}/{fileCount}] - 100% done");

                    directoryInfo.Refresh();
                    stopwatch.Stop();
                    StartViewModel.Log($"Cleared folder: {_recordsFolder} in {Math.Round(stopwatch.Elapsed.TotalSeconds * 100) / 100} seconds");
                    stopwatch.Reset();
                }
                else
                {
                    StartViewModel.Log($"Folder {_recordsFolder} does not exist.");
                    _recordsFolder = string.Empty;
                }
            }
            catch (Exception)
            {
                StartViewModel.Log($"Error with clearing {_recordsFolder}", ConsoleLineOption.Error);
            }
        }

        public override void UpdateStatus()
        {
            if ((IsTracking) || Pid != -1)
            {
                Application.Current?.Dispatcher.Invoke(delegate { StatusLabelColor = new SolidColorBrush(Color.FromRgb(51, 204, 51)); });
                Status = "OPENED";
            }
            else
            {
                Application.Current?.Dispatcher.Invoke(delegate { StatusLabelColor = new SolidColorBrush(Color.FromRgb(125, 38, 37)); });
                Status = "CLOSED";
            }
        }
        protected override void UpdateTitle()
        {
            if (!string.IsNullOrEmpty(WindowTitle)) return;
            WindowTitle = Name;
        }

        protected override void OpenOpenedPathFolder()
        {
            Process.Start("explorer.exe", _recordsFolder!);
        }

        private TKey? GetFirstKey<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
        {
            foreach (var key in dictionary.Keys)
                return key;
            return default;
        }
    }
}