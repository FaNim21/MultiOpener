using MultiOpener.Entities.Misc;
using MultiOpener.Entities.Open;
using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MultiOpener.Entities.Opened.ResetTracker;

/// <summary>
/// Stats Example to do:
/// - wyciaganie seeda z level.dat z folderu swiata
/// </summary>
public sealed class ResetTrackerLocal : OpenedResetTrackerProcess
{
    private readonly Stopwatch _stopwatch = new();
    private readonly FileSystemWatcher _fileWatcher;
    private readonly string _trackerPath;
    private readonly string _trackerPathAPI;

    public RecordType RecordType { get; set; }
    private readonly string _recordType;

    private string? _recordsFolder;
    private DateTime _prevDateTime;
    private const int _breakThreshold = 30;

    private int wallResetsSincePrev;
    private long wallTimeSincePrev;
    private int playedSincePrev;
    private long rtaSincePrev;
    private long breakTimeSincePrev;

    private long lastNetherEntherTimeSession;
    private bool sessionStarted;

    //TODO: 1 zrobic oddzielna klase dla trackerAPI i zrobic tam liste txt'kow do zapisywania i cala logike pod pierwsze tworzenie tych plikow jak i resetowanie itp itd


    public ResetTrackerLocal(RecordType recordType) : base()
    {
        RecordType = recordType;
        _recordType = RecordType.RSG == recordType ? "random_seed" : "set_seed";

        _recordsFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "speedrunigt", "records");
        _trackerPath = System.IO.Path.Combine(Consts.AppdataPath, "Tracker");
        _trackerPathAPI = System.IO.Path.Combine(Consts.AppdataPath, "TrackerAPI");

        _fileWatcher = new FileSystemWatcher
        {
            Path = _recordsFolder!,
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
            Filter = "*.json",
            InternalBufferSize = 16384
        };
        _fileWatcher.Created += OnRecordCreated;

        if (!Directory.Exists(_trackerPath)) Directory.CreateDirectory(_trackerPath);
        if (!Directory.Exists(_trackerPathAPI)) Directory.CreateDirectory(_trackerPathAPI);
    }

    public override void ActivateTracker()
    {
        if (IsTracking) return;
        if (App.Config.DeleteAllRecordOnActivating) ClearRecordsFolder();

        //OnTracking();
        _fileWatcher.EnableRaisingEvents = true;
        IsTracking = true;
        UpdateStatus();

        StartViewModel.Log("Activated Tracker");
    }
    public override void DeactivateTracker()
    {
        if (!IsTracking) return;
        IsTracking = false;
        _fileWatcher.EnableRaisingEvents = false;
        sessionStarted = false;

        _stopwatch.Stop();
        UpdateUIStats();
        _stopwatch.Reset();
        SaveSession();
        SessionData.Clear();
        StartViewModel.Log("Deactivated Tracker");
    }

    private void OnRecordCreated(object sender, FileSystemEventArgs e)
    {
        lock (this)
        {
            string path = e.FullPath;
            string text = ReadAllTextWithRetry(path);

            if (string.IsNullOrEmpty(text)) return;
            try
            {
                RecordData? data = JsonSerializer.Deserialize<RecordData>(text);
                if (data == null) return;
                if (!data.Type!.Equals(_recordType) || data.DefaultGameMode != 0) return;
                if (data.OpenLanTime == null && data.IsCheatAllowed) return;

                if (!sessionStarted) StartSession();
                FilterResetData(data);
            }
            catch (JsonException ex)
            {
                StartViewModel.Log($"Error deserializing {path}: {ex.Message}", ConsoleLineOption.Error);
            }

            SessionData.Update();
            WriteSessionStatsToFile();
        }
    }

    private void StartSession()
    {
        sessionStarted = true;
        Task.Run(UIUpdate, _token);
        StartViewModel.Log("Activated Session");
    }

    private void FilterResetData(RecordData data)
    {
        List<(string name, long IGT, long RTA)> timeLines = new();
        bool foundIronPick = false;
        bool foundEnterNether = false;
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
                {
                    breakTimeSincePrev += (long)runDiffer.TotalMilliseconds;
                    SessionData.BreakTimeMiliseconds += (long)runDiffer.TotalMilliseconds;
                }
                else
                {
                    wallTimeSincePrev += (long)runDiffer.TotalMilliseconds;
                    SessionData.WallTimeMiliseconds += (long)runDiffer.TotalMilliseconds;
                }
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
        RecordStatsCategoriesData? statsData = data.Stats[key].StatsData;
        if (statsData == null) return;

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
                }
            }
        }

        //STATS
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
            }
            else if (data.Advancements != null &&
                     data.Advancements.TryGetValue("minecraft:recipes/misc/iron_nugget_from_smelting", out RecordAdvancementsData? ironNuggetAdvancement) &&
                     ironNuggetAdvancement.IsCompleted &&
                     ironNuggetAdvancement.Criteria!.TryGetValue("has_iron_axe", out var ironAxeTime) &&
                     lanTime > ironAxeTime.RTA)
            {
                SessionData.IronPickaxeCount += 1;
                foundIronPick = true;
            }
        }

        //TIMELINES
        if (data.Timelines != null && data.Timelines.Length != 0)
        {
            int startIndex = -1;
            for (int i = 0; i < data.Timelines.Length; i++)
            {
                var current = data.Timelines[i];
                if (current.Name!.Equals("enter_nether"))
                {
                    startIndex = i;
                    break;
                }
            }

            if (startIndex >= 0)
            {
                foundEnterNether = true;
                for (int j = startIndex; j < data.Timelines?.Length; j++)
                {
                    RecordTimelinesData? prev = j - 1 >= 0 ? data.Timelines[j - 1] : null;
                    RecordTimelinesData? current = data.Timelines[j];
                    string name = current.Name!;

                    if (lanTime < current.RTA) continue;

                    if (name.Equals("enter_fortress") && prev != null && prev.Name!.Equals("enter_nether"))
                        name = "enter_bastion";
                    else if (name.Equals("enter_bastion") && prev != null && prev.Name!.Equals("enter_fortress"))
                        name = "enter_fortress";

                    timeLines.Add((name, current.IGT, current.RTA));
                }
            }
        }

        //splitless so no any evidence of run played
        if (!foundEnterNether)
        {
            if (!foundIronPick)
                SessionData.SplitlessResets++;
            else
                SessionData.NoNetherEnterResets++;

            playedSincePrev += 1;
            rtaSincePrev += data.FinalRTA;
            SessionData.TotalRTAPlayTimeMiliseconds += data.FinalRTA;
            return;
        }

        if (!foundIronPick) SessionData.NetherWithoutPickaxeReset++;

        CreateRunData(data, statsData, timeLines);
    }
    private void CreateRunData(RecordData data, RecordStatsCategoriesData statsData, List<(string name, long IGT, long RTA)> timeLines)
    {
        TrackedRunStats trackedRun = new();

        for (int i = 0; i < timeLines.Count; i++)
        {
            var (name, IGT, _) = timeLines[i];
            SessionData.UpdateSplit(name, IGT);
        }

        trackedRun.Date = DateTimeOffset.FromUnixTimeMilliseconds(data.Date).ToString("yyyy-MM-dd HH:mm:ss");
        trackedRun.TimeZone = TimeZoneInfo.Local.Id;

        trackedRun.RTA = GetTimeFormatMinutes(data.FinalRTA);
        trackedRun.IGT = GetTimeFormatMinutes(data.FinalIGT);
        trackedRun.RetimedIGT = GetTimeFormatMinutes(data.RetimedIGT);

        for (int i = 0; i < timeLines.Count; i++)
        {
            var (name, IGT, _) = timeLines[i];
            int index;
            switch (name)
            {
                case "enter_nether":
                    trackedRun.NetherTime = GetTimeFormatMinutes(IGT);
                    break;
                case "enter_bastion":
                    trackedRun.Structure1 = GetTimeFormatMinutes(IGT);
                    index = name.IndexOf('_');
                    if (index >= 0 && index < name.Length - 1)
                        trackedRun.Structure1Name = name[(index + 1)..];
                    break;
                case "enter_fortress":
                    trackedRun.Structure2 = GetTimeFormatMinutes(IGT);
                    index = name.IndexOf('_');
                    if (index >= 0 && index < name.Length - 1)
                        trackedRun.Structure2Name = name[(index + 1)..];
                    break;
                case "nether_travel":
                    trackedRun.NetherExit = GetTimeFormatMinutes(IGT);
                    break;
                case "enter_stronghold":
                    trackedRun.Stronghold = GetTimeFormatMinutes(IGT);
                    break;
                case "enter_end":
                    trackedRun.EndEnter = GetTimeFormatMinutes(IGT);
                    break;
            }
        }

        if (statsData.PickedUp != null)
        {
            if (statsData.PickedUp.TryGetValue("minecraft:blaze_rod", out int blazeRods))
                trackedRun.BlazeRods = blazeRods;
        }
        if (statsData.Killed != null)
        {
            if (statsData.Killed.TryGetValue("minecraft:blaze", out int killedBlazes))
                trackedRun.KilledBlazes = killedBlazes;
        }
        if (statsData.Custom != null)
        {
            if (statsData.Custom.TryGetValue("minecraft:deaths", out int deaths))
                trackedRun.Deaths = deaths;
        }
        if (statsData.Used != null)
        {
            if (statsData.Used.TryGetValue("minecraft:obsidian", out int obsidian))
                trackedRun.ObsidianPlaced = obsidian;
            if (statsData.Used.TryGetValue("minecraft:ender_eye", out int enderEye))
                trackedRun.EnderEyeUsed = enderEye;
        }

        trackedRun.TimeSincePrevious = GetTimeFormatHours(_stopwatch.ElapsedMilliseconds - lastNetherEntherTimeSession - breakTimeSincePrev);
        lastNetherEntherTimeSession = _stopwatch.ElapsedMilliseconds;

        trackedRun.WallResetsSincePrevious = wallResetsSincePrev;
        trackedRun.PlayedSincePrev = playedSincePrev;

        trackedRun.BreakTimeSincePrevious = GetTimeFormatHours(breakTimeSincePrev);
        trackedRun.WallTimeSincePrevious = GetTimeFormatHours(wallTimeSincePrev);
        trackedRun.RTASincePrevious = GetTimeFormatHours(rtaSincePrev);

        SessionData.TotalRTAPlayTimeMiliseconds += data.FinalRTA;
        SessionData.PostNetherTimeMiliseconds += data.FinalRTA - timeLines[0].RTA;

        breakTimeSincePrev = 0;
        wallTimeSincePrev = 0;
        rtaSincePrev = 0;
        wallResetsSincePrev = 0;
        playedSincePrev = 0;

        SessionData.AddNewRun(trackedRun);

        StartViewModel.Log("==================" + timeLines[0].name + "[{" + GetTimeFormatMinutes(timeLines[0].IGT) + "}] - first split ==================", ConsoleLineOption.Warning);
        StartViewModel.Log(trackedRun.TimeSincePrevious + " - time since last nether enter without breaks");
        StartViewModel.Log(trackedRun.PlayedSincePrev + " - amount played since last nether");
        StartViewModel.Log(trackedRun.RTASincePrevious + " - RTA time since last nether");
        StartViewModel.Log(trackedRun.WallResetsSincePrevious + " - amount wall resets since last nether");
        StartViewModel.Log(trackedRun.WallTimeSincePrevious + " - wall time");
        StartViewModel.Log(trackedRun.BreakTimeSincePrevious + " - break time");
    }

    private async Task UIUpdate()
    {
        _stopwatch.Start();

        while (IsTracking)
        {
            UpdateUIStats();
            await Task.Delay(TimeSpan.FromMilliseconds(100));
        }
    }
    private void UpdateUIStats()
    {
        SessionData.UpdateTimes(_stopwatch.ElapsedMilliseconds);
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
        UpdateFileContent("RNPH", SessionData.RealNetherPerHour);
        UpdateFileContent("LNPH", SessionData.RealNetherPerHour);

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

    private string GetTimeFormatHours(long timeMiliseconds)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(timeMiliseconds);
        return string.Format("{0:D2}:{1:D2}.{2:D2}.{3:D1}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds / 100);
    }
    private string GetTimeFormatMinutes(long timeMiliseconds)
    {
        TimeSpan timeSpan = TimeSpan.FromMilliseconds(timeMiliseconds);
        return string.Format("{0:D2}:{1:D2}.{2:D1}", (int)timeSpan.TotalMinutes, timeSpan.Seconds, timeSpan.Milliseconds / 100);
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

    private string ReadAllTextWithRetry(string path)
    {
        int maxRetries = 5;
        int retries = 0;

        while (retries < maxRetries)
        {
            try
            {
                return File.ReadAllText(path);
            }
            catch
            {
                retries++;
                Task.Delay(100).Wait();
            }
        }

        StartViewModel.Log("Error reading file", ConsoleLineOption.Error);
        return string.Empty;
    }
}