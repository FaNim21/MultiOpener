using MultiOpener.Commands;
using MultiOpener.Entities.Misc;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiOpener.Entities.Opened;


/// <summary>
/// For this moment i would like to make main json that contains all runs collected by built it tracker
/// But main points for now in tracker are:
/// - It only track nether enter runs, because of no need to check for iron/wood stats tbh
/// - It not track which type of biomes you spawn or you enter to nether and with no enter types
/// - 
/// 
/// Stats Example to do:
/// - Time in wall
/// - Time playing (RTA)
/// - Time needed to enter nether
/// - Average for all splits
/// - time played stats
/// </summary>
public class ResetStatsViewModel : BaseViewModel
{
    public long LastFileDateRead { get; set; }

    public bool UsingBuiltIn { get; set; }

    #region Times
    private long _elapsedTimeMiliseconds;
    public long ElapsedTimeMiliseconds
    {
        get => _elapsedTimeMiliseconds;
        set
        {
            _elapsedTimeMiliseconds = value;

            TimeSpan time = TimeSpan.FromMilliseconds(_elapsedTimeMiliseconds);
            ElapsedTime = string.Format("{0:D2}:{1:D2}.{2:D2}.{3:D1}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds / 100);
        }
    }

    private string? _elapsedTime;
    public string? ElapsedTime
    {
        get => _elapsedTime;
        set
        {
            if (_elapsedTime == value) return;

            _elapsedTime = value;
            OnPropertyChanged(nameof(ElapsedTime));
        }
    }

    private long _totalRTAPlayTimeMiliseconds;
    public long TotalRTAPlayTimeMiliseconds
    {
        get => _totalRTAPlayTimeMiliseconds;
        set
        {
            _totalRTAPlayTimeMiliseconds = value;

            TimeSpan time = TimeSpan.FromMilliseconds(_totalRTAPlayTimeMiliseconds);
            TotalRTAPlayTime = string.Format("{0:D2}:{1:D2}.{2:D2}.{3:D1}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds / 100);
        }
    }

    private string? _totalRTAPlayTime;
    public string? TotalRTAPlayTime
    {
        get => _totalRTAPlayTime;
        set
        {
            _totalRTAPlayTime = value;
            OnPropertyChanged(nameof(ElapsedTime));
        }
    }
    #endregion

    #region Resets
    private int _wallResets;
    public int WallResets
    {
        get { return _wallResets; }
        set
        {
            _wallResets = value;
            OnPropertyChanged(nameof(Resets));
        }
    }

    private int _noNetherEnterResets;
    public int NoNetherEnterResets
    {
        get { return _noNetherEnterResets; }
        set
        {
            _noNetherEnterResets = value;
            OnPropertyChanged(nameof(Resets));
        }
    }

    private int _splitlessResets;
    public int SplitlessResets
    {
        get { return _splitlessResets; }
        set
        {
            _splitlessResets = value;
            OnPropertyChanged(nameof(Resets));
        }
    }

    public int Resets => WallResets + NoNetherEnterResets + SplitlessResets;
    #endregion

    #region Splits
    //IRON PICKAXE
    private int _ironPickaxeCount;
    public int IronPickaxeCount
    {
        get { return _ironPickaxeCount; }
        set
        {
            _ironPickaxeCount = value;
            OnPropertyChanged(nameof(IronPickaxeCount));
        }
    }


    //NETHER
    private int _netherEntersCount;
    public int NetherEntersCount
    {
        get { return _netherEntersCount; }
        set
        {
            _netherEntersCount = value;
            OnPropertyChanged(nameof(NetherEntersCount));
        }
    }

    private long _netherEntersTime;
    public long NetherEntersTime
    {
        get { return _netherEntersTime; }
        set
        {
            _netherEntersTime = value;
            OnPropertyChanged(nameof(NetherEnterAverageTime));
        }
    }

    public string NetherEnterAverageTime
    {
        get
        {
            float miliseconds;
            if (UsingBuiltIn) miliseconds = NetherEntersCount == 0 ? 0 : NetherEntersTime / NetherEntersCount;
            else miliseconds = NetherEntersTime;
            TimeSpan time = TimeSpan.FromMilliseconds(miliseconds);
            string formattedTime = string.Format("{0:D2}:{1:D2}.{2:D1}", time.Minutes, time.Seconds, time.Milliseconds / 100);
            return formattedTime;
        }
    }


    //STRUCTURE 1
    private int _firstStructureEntersCount;
    public int FirstStructureEntersCount
    {
        get { return _firstStructureEntersCount; }
        set
        {
            _firstStructureEntersCount = value;
            OnPropertyChanged(nameof(FirstStructureEntersCount));
        }
    }

    private long _firstStructureEntersTime;
    public long FirstStructureEntersTime
    {
        get { return _firstStructureEntersTime; }
        set
        {
            _firstStructureEntersTime = value;
            OnPropertyChanged(nameof(FirstStructureEnterAverageTime));
        }
    }

    public string FirstStructureEnterAverageTime
    {
        get
        {
            float miliseconds;
            if (UsingBuiltIn) miliseconds = FirstStructureEntersCount == 0 ? 0 : FirstStructureEntersTime / FirstStructureEntersCount;
            else miliseconds = FirstStructureEntersTime;
            TimeSpan time = TimeSpan.FromMilliseconds(miliseconds);
            string formattedTime = string.Format("{0:D2}:{1:D2}.{2:D1}", time.Minutes, time.Seconds, time.Milliseconds / 100);
            return formattedTime;
        }
    }


    //STRUCTURE 1
    private int _secondStructureEntersCount;
    public int SecondStructureEntersCount
    {
        get { return _secondStructureEntersCount; }
        set
        {
            _secondStructureEntersCount = value;
            OnPropertyChanged(nameof(SecondStructureEntersCount));
        }
    }

    private long _secondStructureEntersTime;
    public long SecondStructureEntersTime
    {
        get { return _secondStructureEntersTime; }
        set
        {
            _secondStructureEntersTime = value;
            OnPropertyChanged(nameof(SecondStructureEnterAverageTime));
        }
    }

    public string SecondStructureEnterAverageTime
    {
        get
        {
            float miliseconds;
            if (UsingBuiltIn) miliseconds = SecondStructureEntersCount == 0 ? 0 : SecondStructureEntersTime / SecondStructureEntersCount;
            else miliseconds = SecondStructureEntersTime;
            TimeSpan time = TimeSpan.FromMilliseconds(miliseconds);
            string formattedTime = string.Format("{0:D2}:{1:D2}.{2:D1}", time.Minutes, time.Seconds, time.Milliseconds / 100);
            return formattedTime;
        }
    }


    //NETHER EXIT
    private int _netherExitEntersCount;
    public int NetherExitEntersCount
    {
        get { return _netherExitEntersCount; }
        set
        {
            _netherExitEntersCount = value;
            OnPropertyChanged(nameof(NetherExitEntersCount));
        }
    }

    private long _netherExitEntersTime;
    public long NetherExitEntersTime
    {
        get { return _netherExitEntersTime; }
        set
        {
            _netherExitEntersTime = value;
            OnPropertyChanged(nameof(NetherExitEnterAverageTime));
        }
    }

    public string NetherExitEnterAverageTime
    {
        get
        {
            float miliseconds;
            if (UsingBuiltIn) miliseconds = NetherExitEntersCount == 0 ? 0 : NetherExitEntersTime / NetherExitEntersCount;
            else miliseconds = NetherExitEntersTime;
            TimeSpan time = TimeSpan.FromMilliseconds(miliseconds);
            string formattedTime = string.Format("{0:D2}:{1:D2}.{2:D1}", time.Minutes, time.Seconds, time.Milliseconds / 100);
            return formattedTime;
        }
    }


    //STRONGHOLD
    private int _strongholdEntersCount;
    public int StrongholdEntersCount
    {
        get { return _strongholdEntersCount; }
        set
        {
            _strongholdEntersCount = value;
            OnPropertyChanged(nameof(StrongholdEntersCount));
        }
    }

    private long _strongholdEntersTime;
    public long StrongholdEntersTime
    {
        get { return _strongholdEntersTime; }
        set
        {
            _strongholdEntersTime = value;
            OnPropertyChanged(nameof(StrongholdEnterAverageTime));
        }
    }

    public string StrongholdEnterAverageTime
    {
        get
        {
            float miliseconds;
            if (UsingBuiltIn) miliseconds = StrongholdEntersCount == 0 ? 0 : StrongholdEntersTime / StrongholdEntersCount;
            else miliseconds = StrongholdEntersTime;
            TimeSpan time = TimeSpan.FromMilliseconds(miliseconds);
            string formattedTime = string.Format("{0:D2}:{1:D2}.{2:D1}", time.Minutes, time.Seconds, time.Milliseconds / 100);
            return formattedTime;
        }
    }


    //END
    private int _endEntersCount;
    public int EndEntersCount
    {
        get { return _endEntersCount; }
        set
        {
            _endEntersCount = value;
            OnPropertyChanged(nameof(EndEntersCount));
        }
    }

    private long _endEntersTime;
    public long EndEntersTime
    {
        get { return _endEntersTime; }
        set
        {
            _endEntersTime = value;
            OnPropertyChanged(nameof(EndEnterAverageTime));
        }
    }

    public string EndEnterAverageTime
    {
        get
        {
            float miliseconds;
            if (UsingBuiltIn) miliseconds = EndEntersCount == 0 ? 0 : EndEntersTime / EndEntersCount;
            else miliseconds = EndEntersTime;
            TimeSpan time = TimeSpan.FromMilliseconds(miliseconds);
            string formattedTime = string.Format("{0:D2}:{1:D2}.{2:D1}", time.Minutes, time.Seconds, time.Milliseconds / 100);
            return formattedTime;
        }
    }
    #endregion

    #region Basic stats
    private float _netherPerHour;
    public float NetherPerHour
    {
        get => _netherPerHour;
        set
        {
            _netherPerHour = value;
            OnPropertyChanged(nameof(NetherPerHour));
        }
    }

    #endregion

    public void UpdateSplit(string splitName, long time)
    {
        switch (splitName)
        {
            case "enter_nether":
                NetherEntersCount += 1;
                NetherEntersTime += time;
                break;
            case "enter_bastion":
                FirstStructureEntersCount += 1;
                FirstStructureEntersTime += time;
                break;
            case "enter_fortress":
                SecondStructureEntersCount += 1;
                SecondStructureEntersTime += time;
                break;
            case "nether_travel":
                NetherExitEntersCount += 1;
                NetherExitEntersTime += time;
                break;
            case "enter_stronghold":
                StrongholdEntersCount += 1;
                StrongholdEntersTime += time;
                break;
            case "enter_end":
                EndEntersCount += 1;
                EndEntersTime += time;
                break;
        }
    }

    public void UpdatePerHourStats()
    {
        //nethers
        float ratio = NetherEntersCount / (ElapsedTimeMiliseconds / 3_600_000f);
        NetherPerHour = (float)Math.Round(ratio, 2);


        //..
    }

    public void Clear()
    {
        LastFileDateRead = 0;

        ElapsedTimeMiliseconds = 0;
        TotalRTAPlayTimeMiliseconds = 0;

        NoNetherEnterResets = 0;
        WallResets = 0;

        IronPickaxeCount = 0;

        NetherEntersCount = 0;
        NetherEntersTime = 0;

        FirstStructureEntersCount = 0;
        FirstStructureEntersTime = 0;

        SecondStructureEntersCount = 0;
        SecondStructureEntersTime = 0;

        NetherExitEntersCount = 0;
        NetherExitEntersTime = 0;

        StrongholdEntersCount = 0;
        StrongholdEntersTime = 0;

        EndEntersCount = 0;
        EndEntersTime = 0;
    }
}

public sealed class OpenedResetTrackerProcess : OpenedProcess
{
    private readonly struct ResetAnalyticsResponse
    {
        public bool success { get; init; }
        public List<Session> session { get; init; }
    }
    private readonly struct Session
    {
        public Ops ops { get; init; }
    }
    private readonly struct Ops
    {
        public Tl[] tl { get; init; }
        public int rc { get; init; }

    }
    private readonly struct Tl
    {
        public int time { get; init; }
        public int total { get; init; }
    }


    private bool _isTracking;
    public bool IsTracking
    {
        get => _isTracking;
        set
        {
            _isTracking = value;
            OnPropertyChanged(nameof(IsTracking));
        }
    }

    private int _timeToUpdateStats;
    public int TimeToUpdateStats
    {
        get => _timeToUpdateStats;
        set
        {
            _timeToUpdateStats = value;
            OnPropertyChanged(nameof(TimeToUpdateStats));
        }
    }

    public ICommand ForceUpdateCommand { get; set; }

    private string _trackerId = string.Empty;
    private bool _usingBuiltInTracker = true;
    private string? _recordsFolder;
    private int updateFrequencySize;
    private long uiUpdateCount = 0;

    private readonly Stopwatch stopwatch = new();

    public ResetStatsViewModel ResetData { get; set; } = new();

    private CancellationTokenSource _source = new();
    private CancellationToken _token;
    private Task? _trackerTask;


    public OpenedResetTrackerProcess()
    {
        ForceUpdateCommand = new RelayCommand(ForceUpdateStats);
    }

    public void Setup(string trackerID, bool usingBuiltInTracker)
    {
        _trackerId = trackerID;
        _usingBuiltInTracker = usingBuiltInTracker;
        ResetData.UsingBuiltIn = usingBuiltInTracker;

        //TODO: 0 Tymczasowo do statystyk sesji w main resettrackerviewmodel przed zrobieniem zapisywania runow
        Application.Current?.Dispatcher.Invoke(delegate
        {
            MainViewModel mainViewModel = ((MainWindow)Application.Current.MainWindow).MainViewModel;
            ResetTrackerViewModel? resetTrackerViewModel = mainViewModel.GetViewModel<ResetTrackerViewModel>();
            if (resetTrackerViewModel != null)
                resetTrackerViewModel.ResetTracker = this;
        });
    }

    public void ActivateTracker()
    {
        if (IsTracking) return;

        if (_usingBuiltInTracker)
        {
            _recordsFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "speedrunigt", "records");
            if (App.Config.DeleteAllRecordOnActivating)
                ClearRecordsFolder();
        }

        _source = new();
        _token = _source.Token;

        IsTracking = true;
        UpdateStatus();

        StartViewModel.Log("Activated Tracker");
        _trackerTask = Task.Run(TrackStats, _token);
        _ = Task.Run(UIUpdate, _token);
    }
    public void DeactivateTracker()
    {
        if (_token.IsCancellationRequested || !_isTracking) return;

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
        ResetStatsData();
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
                if (_usingBuiltInTracker)
                    OnBuiltInTracker();
                else
                    await OnOutsideTracker();

                TimeToUpdateStats = updateFrequencySize;
            }
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

    private void OnBuiltInTracker()
    {
        if (string.IsNullOrEmpty(_recordsFolder)) return;

        long lastFileOpenedRead = ResetData.LastFileDateRead;
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
                if (data.Date <= ResetData.LastFileDateRead) return;
                if (data.Date >= lastFileOpenedRead) lastFileOpenedRead = data.Date;
                if (!data.Type!.Equals("random_seed") || data.DefaultGameMode != 0) return;
                if (data.OpenLanTime == null && data.IsCheatAllowed) return;

                FilterResetData(data);
            }
            catch (JsonException ex)
            {
                StartViewModel.Log($"Error deserializing {records[i]}: {ex.Message}", ConsoleLineOption.Error);
            }
        }
        ResetData.LastFileDateRead = lastFileOpenedRead;
    }

    private void FilterResetData(RecordData data)
    {
        bool foundIronPick = false;
        if (data.FinalRTA == 0)
        {
            ResetData.WallResets += 1;
            return;
        }

        //ADVANCEMENTS
        if (data.Advancements != null && data.Advancements.Count != 0)
        {
            //IRON PICK
            if (data.Advancements.TryGetValue("minecraft:story/iron_tools", out var story) && story.IsCompleted)
            {
                ResetData.IronPickaxeCount += 1;
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
                ResetData.IronPickaxeCount += 1;
                foundIronPick = true;
            }
        }

        //MAIN SPLITS
        if (data.Timelines == null || data.Timelines.Length == 0)
        {
            ResetData.NoNetherEnterResets += 1;
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
                        ResetData.SplitlessResets += 1;
                        break;
                    }
                }
            }

            if (name.Equals("enter_fortress") && prev != null && prev.Name!.Equals("enter_nether"))
                name = "enter_bastion";
            else if (name.Equals("enter_bastion") && prev != null && prev.Name!.Equals("enter_fortress"))
                name = "enter_fortress";

            ResetData.UpdateSplit(name, current.IGT);
        }
    }

    private async Task OnOutsideTracker()
    {
        string apiUrl = $"https://reset-analytics.vercel.app/api/sheet/{_trackerId}";

        try
        {

            using var httpClient = new HttpClient();
            HttpResponseMessage response = await httpClient.GetAsync(apiUrl, _token);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync(_token);
                ResetAnalyticsResponse apiResponse = JsonSerializer.Deserialize<ResetAnalyticsResponse>(responseBody)!;

                if (apiResponse.success)
                {
                    Ops ops = apiResponse.session[0].ops;
                    ResetData.WallResets = ops.rc;

                    Tl[] tl = ops.tl;

                    ResetData.NetherEntersCount = tl[3].total;
                    ResetData.NetherEntersTime = tl[3].time;

                    ResetData.FirstStructureEntersCount = tl[4].total;
                    ResetData.FirstStructureEntersTime = tl[4].time;

                    ResetData.SecondStructureEntersCount = tl[5].total;
                    ResetData.SecondStructureEntersTime = tl[5].time;

                    ResetData.NetherExitEntersCount = tl[6].total;
                    ResetData.NetherExitEntersTime = tl[6].time;

                    ResetData.StrongholdEntersCount = tl[7].total;
                    ResetData.StrongholdEntersTime = tl[7].time;

                    ResetData.EndEntersCount = tl[8].total;
                    ResetData.EndEntersTime = tl[8].time;
                }
            }
            else
            {
                StartViewModel.Log("Error fetching stats: " + response.StatusCode, ConsoleLineOption.Error);
            }
        }
        catch
        {
            StartViewModel.Log("Error fetching stats", ConsoleLineOption.Error);
        }
    }

    private void UpdateUIStats()
    {
        //every 100 miliseconds
        uiUpdateCount++;
        ResetData.ElapsedTimeMiliseconds = stopwatch.ElapsedMilliseconds;

        if (uiUpdateCount % 10 == 0) //every second
        {
            ResetData.UpdatePerHourStats();
        }
    }

    protected override void UpdateTitle()
    {
        if (!_usingBuiltInTracker)
        {
            base.UpdateTitle();
            return;
        }

        if (!string.IsNullOrEmpty(WindowTitle)) return;
        WindowTitle = Name;
    }
    public override void UpdateStatus()
    {
        if ((_usingBuiltInTracker && IsTracking) || Pid != -1)
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

    public override async Task<bool> OpenProcess(CancellationToken token = default)
    {
        if (!_usingBuiltInTracker)
            await base.OpenProcess(token);

        ActivateTracker();
        return await Task.FromResult(true);
    }

    public override async Task<bool> Close()
    {
        DeactivateTracker();

        if (Pid == -1)
        {
            Clear();
            return true;
        }

        try
        {
            bool output = false;

            if (!Win32.IsProcessResponding(Pid))
                output = await Win32.CloseProcessByPid(Pid);

            if (!output)
            {
                output = await Win32.CloseProcessByHwnd(Hwnd);
                if (!output)
                    output = await Win32.CloseProcessByPid(Pid);
            }

            Clear();
            return output;
        }
        catch (Exception e)
        {
            StartViewModel.Log($"Cannot close MC instance named {Name}(Title: {WindowTitle}) \n{e}", ConsoleLineOption.Error);
            return false;
        }
    }

    private void ResetStatsData()
    {
        ResetData.Clear();
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

    protected override void OpenOpenedPathFolder()
    {
        if (_usingBuiltInTracker)
        {
            Process.Start("explorer.exe", _recordsFolder!);
            return;
        }
        base.OpenOpenedPathFolder();
    }

    private void ForceUpdateStats()
    {
        if (!IsTracking) return;

        TimeToUpdateStats = 0;
    }

    private TKey? GetFirstKey<TKey, TValue>(Dictionary<TKey, TValue> dictionary)
    {
        foreach (var key in dictionary.Keys)
            return key;
        return default;
    }
}
