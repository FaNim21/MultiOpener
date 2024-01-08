using MultiOpener.Entities.Misc;
using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MultiOpener.Entities.Opened.ResetTracker
{
    /// <summary>
    /// Stats Example to do:
    /// - Time needed to enter nether
    /// - Average for all splits
    /// - wyciaganie seeda z level.dat z folderu swiata
    /// 
    /// ---- UWZGLEDNIC TO ZEBY NA PIERWSZYM RUNIE SESJI NIE USTAWIALO STATYSTYK JAK since previously ------
    /// </summary>
    public sealed class ResetTrackerLocal : OpenedResetTrackerProcess
    {
        private readonly Stopwatch stopwatch = new();
        private readonly string _trackerPath;
        private readonly string _trackerPathAPI;

        private string? _recordsFolder;
        private DateTime _prevDateTime;
        private const int _breakThreshold = 30;

        private long breakTime;
        private long wallTime;

        private long rtaSincePrev;
        private int wallResetsSincePrev;
        private int playedSincePrev;

        private long lastNetherEntherTimeSession;

        //TODO: 0 zle liczy total rta i trzeba naprawic te pickaxy i robienie nonetherenter--;


        public ResetTrackerLocal() : base()
        {
            _trackerPath = System.IO.Path.Combine(Consts.AppdataPath, "Tracker");
            _trackerPathAPI = System.IO.Path.Combine(Consts.AppdataPath, "TrackerAPI");

            if (!Directory.Exists(_trackerPath)) Directory.CreateDirectory(_trackerPath);
            if (!Directory.Exists(_trackerPathAPI)) Directory.CreateDirectory(_trackerPathAPI);
        }

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
            SaveSession();
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
            for (int i = 0; i < records.Length; i++)
            {
                if (_token.IsCancellationRequested) break;

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
            lock (this)
            {
                SessionData.Update();
                WriteSessionStatsToFile();
            }
        }

        private void FilterResetData(RecordData data)
        {
            TrackedRunStats trackedRun = new();
            List<(string name, long IGT)> timeLines = new();
            bool foundIronPick = false;
            bool hasDoneSomething = false;
            TimeSpan runDiffer;

            //BREAK TIMES
            if (_prevDateTime != DateTime.MinValue)
            {
                runDiffer = (DateTime.Now - _prevDateTime) - TimeSpan.FromMilliseconds(data.FinalRTA);
                if (runDiffer < TimeSpan.Zero)
                {
                    data.FinalRTA = data.FinalIGT;
                    runDiffer = (DateTime.Now - _prevDateTime) - TimeSpan.FromMilliseconds(data.FinalRTA);
                }

                if (runDiffer > TimeSpan.Zero)
                {
                    if (runDiffer > TimeSpan.FromSeconds(_breakThreshold))
                        breakTime += (long)runDiffer.TotalMilliseconds;
                    else
                        wallTime += (long)runDiffer.TotalMilliseconds;
                }

                _prevDateTime = DateTime.Now;
            }
            else _prevDateTime = DateTime.Now;

            //wall reset run
            if (data.FinalRTA == 0)
            {
                SessionData.WallResets += 1;
                wallResetsSincePrev += 1;
                return;
            }

            //stats validation and lanTime setup
            if (data.Stats == null || data.Stats.Count == 0) return;
            string? key = GetFirstKey(data.Stats);
            if (string.IsNullOrEmpty(key)) return;

            data.OpenLanTime ??= int.MaxValue.ToString();
            long lanTime = long.Parse(data.OpenLanTime.ToString()!);

            //ADVANCEMENTS
            if (data.Advancements != null && data.Advancements.Count != 0)
            {
                //IRON PICK
                if (data.Advancements.TryGetValue("minecraft:story/iron_tools", out var story) && story.IsCompleted && story.Criteria != null && story.Criteria.TryGetValue("iron_pickaxe", out var times))
                {
                    if (lanTime > times.RTA)
                    {
                        SessionData.IronPickaxeCount += 1;
                        foundIronPick = true;
                        hasDoneSomething = true;
                    }
                }
            }

            //STATS
            if (data.Stats[key].StatsData != null)
            {
                RecordStatsCategoriesData statsData = data.Stats[key].StatsData!;
                //DIAMOND PICK
                if (statsData.Crafted != null && statsData.Crafted.TryGetValue("minecraft:diamond_pickaxe", out _) && !foundIronPick)
                {
                    if (data.Advancements != null &&
                        data.Advancements.TryGetValue("minecraft:recipes/misc/gold_nugget_from_smelting", out RecordAdvancementsData? goldNuggetAdvancement) &&
                        goldNuggetAdvancement.IsCompleted &&
                        goldNuggetAdvancement.Criteria!.TryGetValue("has_golden_axe", out var goldenAxeTime) &&
                        lanTime > goldenAxeTime.RTA)
                    {
                        SessionData.IronPickaxeCount += 1;
                        foundIronPick = true;
                        hasDoneSomething = true;
                    }
                    else if (data.Advancements != null &&
                             data.Advancements.TryGetValue("minecraft:recipes/misc/iron_nugget_from_smelting", out RecordAdvancementsData? ironNuggetAdvancement) &&
                             ironNuggetAdvancement.IsCompleted &&
                             ironNuggetAdvancement.Criteria!.TryGetValue("has_iron_axe", out var ironAxeTime) &&
                             lanTime > ironAxeTime.RTA)
                    {
                        SessionData.IronPickaxeCount += 1;
                        foundIronPick = true;
                        hasDoneSomething = true;
                    }
                }
            }

            //TIMELINES
            if (data.Timelines != null && data.Timelines.Length != 0)
            {
                int startIndex = 0;
                bool foundNether = false;

                if (!data.Timelines[0].Name!.Equals("enter_nether"))
                {
                    for (int i = 0; i < data.Timelines.Length; i++)
                    {
                        var current = data.Timelines[i];
                        if (current.Name!.Equals("enter_nether"))
                        {
                            startIndex = i;
                            foundNether = true;
                            break;
                        }
                    }
                    if (!foundNether) startIndex = -1;
                }

                if (startIndex >= 0)
                {
                    for (int j = startIndex; j < data.Timelines?.Length; j++)
                    {
                        RecordTimelinesData? prev = j - 1 >= 0 ? data.Timelines[j - 1] : null;
                        RecordTimelinesData? current = data.Timelines[j];
                        string name = current.Name!;

                        if (lanTime < current.RTA) continue;
                        hasDoneSomething = true;

                        if (name.Equals("enter_fortress") && prev != null && prev.Name!.Equals("enter_nether"))
                            name = "enter_bastion";
                        else if (name.Equals("enter_bastion") && prev != null && prev.Name!.Equals("enter_fortress"))
                            name = "enter_fortress";

                        timeLines.Add((name, current.IGT));
                    }
                }
            }

            //splitless so no any evidence of run played
            if (!hasDoneSomething)
            {
                SessionData.SplitlessResets++;
                rtaSincePrev += data.FinalRTA;
                playedSincePrev += 1;
                return;
            }

            //no nether enter so no split recorded
            if (timeLines.Count == 0 && foundIronPick)
            {
                SessionData.NoNetherEnterResets++;
                rtaSincePrev += data.FinalRTA;
                playedSincePrev += 1;
                return;
            }
            //to zmniejszalo o ilosc netherow bez kilofa
            //TODO: 0 to wszystko naprawic zeby nie odejmowac danych tylko zeby dodawalo wlasciwe
            if (timeLines.Count != 0 && !foundIronPick)
            {
                SessionData.NoNetherEnterResets--;
                rtaSincePrev -= data.FinalRTA;
                playedSincePrev -= 1;
            }


            /*PART FOR UPDATING SESSION ETC*/
            for (int i = 0; i < timeLines.Count; i++)
            {
                var (name, IGT) = timeLines[i];
                SessionData.UpdateSplit(name, IGT);
            }

            trackedRun.Date = DateTimeOffset.FromUnixTimeMilliseconds(data.Date).ToString("yyyy-MM-dd HH:mm:ss");
            trackedRun.TimeZone = TimeZoneInfo.Local.Id;

            trackedRun.RTA = GetTimeFormat(data.FinalRTA);
            trackedRun.IGT = GetTimeFormat(data.FinalIGT);
            trackedRun.RetimedIGT = GetTimeFormat(data.RetimedIGT);

            for (int i = 0; i < timeLines.Count; i++)
            {
                var (name, IGT) = timeLines[i];
                switch (name)
                {
                    case "enter_nether":
                        trackedRun.NetherTime = GetTimeFormat(IGT);
                        break;
                    case "enter_bastion":
                        trackedRun.Structure1 = GetTimeFormat(IGT);
                        break;
                    case "enter_fortress":
                        trackedRun.Structure2 = GetTimeFormat(IGT);
                        break;
                    case "nether_travel":
                        trackedRun.NetherExit = GetTimeFormat(IGT);
                        break;
                    case "enter_stronghold":
                        trackedRun.Stronghold = GetTimeFormat(IGT);
                        break;
                    case "enter_end":
                        trackedRun.EndEnter = GetTimeFormat(IGT);
                        break;
                }
            }

            if (data.Stats[key].StatsData!.PickedUp != null && data.Stats[key].StatsData!.PickedUp!.TryGetValue("minecraft:blaze_rod", out int blazeRods))
                trackedRun.BlazeRods = blazeRods;

            if (data.Stats[key].StatsData!.Killed != null && data.Stats[key].StatsData!.Killed!.TryGetValue("minecraft:blaze", out int killedBlazes))
                trackedRun.KilledBlazes = killedBlazes;

            trackedRun.TimeSincePrevious = GetTimeFormat(stopwatch.ElapsedMilliseconds - lastNetherEntherTimeSession - breakTime);
            lastNetherEntherTimeSession = stopwatch.ElapsedMilliseconds;

            trackedRun.WallResetsSincePrevious = wallResetsSincePrev;
            trackedRun.PlayedSincePrev = playedSincePrev;

            SessionData.BreakTimeMiliseconds += breakTime;
            SessionData.WallTimeMiliseconds += wallTime;

            trackedRun.BreakTimeSincePrevious = GetTimeFormat(breakTime);
            trackedRun.WallTimeSincePrevious = GetTimeFormat(wallTime);
            trackedRun.RTASincePrevious = GetTimeFormat(rtaSincePrev);

            SessionData.TotalRTAPlayTimeMiliseconds += wallTime + rtaSincePrev + data.FinalRTA;

            breakTime = 0;
            wallTime = 0;
            rtaSincePrev = 0;
            wallResetsSincePrev = 0;
            playedSincePrev = 0;

            SessionData.AddNewRun(trackedRun);

            StartViewModel.Log("==================" + timeLines[0].name + "[{" + GetTimeFormat(timeLines[0].IGT) + "}] - first split ==================");
            StartViewModel.Log(trackedRun.TimeSincePrevious + " - time since last nether enter without breaks");
            StartViewModel.Log(trackedRun.WallResetsSincePrevious + " - amount wall resets since last nether");
            StartViewModel.Log(trackedRun.PlayedSincePrev + " - amount played since last nether");
            StartViewModel.Log(trackedRun.BreakTimeSincePrevious + " - break time");
            StartViewModel.Log(trackedRun.WallTimeSincePrevious + " - wall time");
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
            SessionData.UpdateTimes(stopwatch.ElapsedMilliseconds);

            if (uiUpdateCount % 10 == 0) //every second
            {
                SessionData.UpdatePerHourStats();
            }
        }

        private void SaveSession()
        {
            DateTime currentDate = DateTime.Now;
            string formattedDateTime = currentDate.ToString("dd-MM-yyyy_HH.mm.ss");
            var data = JsonSerializer.Serialize(SessionData);
            var path = System.IO.Path.Combine(_trackerPath, $"Session[{formattedDateTime}].json");
            File.WriteAllText(path, data);
        }

        private void WriteSessionStatsToFile()
        {
            UpdateFileContent("Resets", SessionData.Resets);
            UpdateFileContent("NPH", SessionData.NetherPerHour);

            UpdateFileContent("NetherEnter_Average", SessionData.NetherEnterAverageTime);
            UpdateFileContent("NetherEnter_Count", SessionData.NetherEntersCount);

            UpdateFileContent("Structure1_Average", SessionData.FirstStructureEnterAverageTime);
            UpdateFileContent("Structure1_Count", SessionData.FirstStructureEntersCount);

            UpdateFileContent("Structure2_Average", SessionData.SecondStructureEnterAverageTime);
            UpdateFileContent("Structure2_Count", SessionData.SecondStructureEntersCount);

            UpdateFileContent("NetherExit_Average", SessionData.NetherExitEnterAverageTime);
            UpdateFileContent("NetherExit_Count", SessionData.NetherExitEntersCount);

            UpdateFileContent("Stronghold_Average", SessionData.StrongholdEnterAverageTime);
            UpdateFileContent("Stronghold_Count", SessionData.StrongholdEntersCount);

            UpdateFileContent("EndEnter_Average", SessionData.EndEnterAverageTime);
            UpdateFileContent("EndEnter_Count", SessionData.EndEntersCount);
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

        public override Task<bool> OpenProcess(CancellationToken token = default)
        {
            ActivateTracker();
            return Task.FromResult(true);
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

        private string GetTimeFormat(long timeMiliseconds)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(timeMiliseconds);
            return string.Format("{0:D2}:{1:D2}.{2:D2}.{3:D1}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 100);
        }

        private void UpdateFileContent(string fileName, object content)
        {
            string path = System.IO.Path.Combine(_trackerPathAPI, fileName + ".txt");
            if (!File.Exists(path)) File.Create(path);

            try
            {
                using StreamWriter writer = new(path);
                writer.Write(content.ToString());
            }
            catch (Exception ex)
            {
                StartViewModel.Log($"Error updating file {fileName}: {ex.Message}");
            }
        }
    }
}