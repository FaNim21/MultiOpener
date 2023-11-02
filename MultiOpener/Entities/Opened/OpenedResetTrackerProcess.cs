using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using MultiOpener.Entities.Misc;

namespace MultiOpener.Entities.Opened;


public class ResetStats : BaseViewModel
{
    public bool UsingBuiltIn { get; set; }

    public int WallResets { get; set; }

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

    private int _netherEntersTime;
    public int NetherEntersTime
    {
        get { return _netherEntersTime; }
        set { _netherEntersTime = value; }
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

    private int _firstStructureEntersTime;
    public int FirstStructureEntersTime
    {
        get { return _firstStructureEntersTime; }
        set { _firstStructureEntersTime = value; }
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

    private int _secondStructureEntersTime;
    public int SecondStructureEntersTime
    {
        get { return _secondStructureEntersTime; }
        set { _secondStructureEntersTime = value; }
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

    private int _netherExitEntersTime;
    public int NetherExitEntersTime
    {
        get { return _netherExitEntersTime; }
        set { _netherExitEntersTime = value; }
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

    private int _strongholdEntersTime;
    public int StrongholdEntersTime
    {
        get { return _strongholdEntersTime; }
        set { _strongholdEntersTime = value; }
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

    private int _endEntersTime;
    public int EndEntersTime
    {
        get { return _endEntersTime; }
        set { _endEntersTime = value; }
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


    public void Reset()
    {
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
    }
    private readonly struct Tl
    {
        public int time { get; init; }
        public int total { get; init; }
    }


    private bool _isTracking;
    public bool IsTracking
    {
        get { return _isTracking; }
        set
        {
            _isTracking = value;
            OnPropertyChanged(nameof(IsTracking));
        }
    }


    private string trackerID = string.Empty;
    private bool usingBuiltInTracker = true;

    public ResetStats ResetData { get; set; } = new();

    private CancellationTokenSource _source = new();
    private CancellationToken _token;
    private Task? _trackerTask;

    private string? _recordsFolder;


    public void Setup(string trackerID, bool usingBuiltInTracker)
    {
        this.trackerID = trackerID;
        this.usingBuiltInTracker = usingBuiltInTracker;
        ResetData.UsingBuiltIn = usingBuiltInTracker;
    }

    public void ActivateTracker()
    {
        //TODO: 0 przenies activate do po refreshu na koncu odpalania
        if (IsTracking) return;

        if (usingBuiltInTracker)
        {
            _recordsFolder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "speedrunigt", "records");
            ClearResetFolder();
        }

        _source = new();
        _token = _source.Token;

        IsTracking = true;
        UpdateStatus();

        _trackerTask = Task.Run(TrackStats, _token);
    }
    public void DeactivateTracker()
    {
        if (_source == null || _token.IsCancellationRequested || !_isTracking) return;

        _source.Cancel();
        IsTracking = false;

        if (_trackerTask != null && !_trackerTask.IsCompleted)
            _trackerTask.Wait();

        _source.Dispose();
        ResetStatsData();
    }

    private async Task TrackStats()
    {
        while (IsTracking)
        {
            try
            {
                await Task.Delay(App.Config.UpdateResetTrackerFrequency, _token);
            }
            catch { break; }

            if (usingBuiltInTracker)
                await OnBuiltInTracker();
            else
                await OnOutsideTracker();

            StartViewModel.Log("Tracking...");
        }
    }

    private async Task OnBuiltInTracker()
    {
        if (string.IsNullOrEmpty(_recordsFolder)) return;

        //bez sensu metoda od wyciagania listy (od cholery GC), ale to i tak narazie
        List<RecordData> data = GetRecords();
        if (data == null) return;

        for (int i = 0; i < data.Count; i++)
        {

        }

        ClearResetFolder();
    }

    private async Task OnOutsideTracker()
    {
        string apiUrl = $"https://reset-analytics.vercel.app/api/sheet/{trackerID}";

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
                    Tl[] tl = apiResponse.session[0].ops.tl;

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

    public override void Update(bool lookForWindow = false)
    {
        base.Update(lookForWindow);
    }

    public override void UpdateTitle()
    {
        if (!usingBuiltInTracker)
        {
            base.UpdateTitle();
            return;
        }

        if (!string.IsNullOrEmpty(WindowTitle)) return;
        WindowTitle = Name;
    }
    public override void UpdateStatus()
    {
        if ((usingBuiltInTracker && IsTracking) || Pid != -1)
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
        if (!usingBuiltInTracker)
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

    private List<RecordData> GetRecords()
    {
        List<RecordData> datas = new();

        var records = Directory.GetFiles(_recordsFolder!, "*.json", SearchOption.TopDirectoryOnly).AsSpan();
        for (int i = 0; i < records.Length; i++)
        {
            string text = File.ReadAllText(records[i]) ?? string.Empty;
            try
            {
                if (string.IsNullOrEmpty(text)) continue;

                RecordData? data = JsonSerializer.Deserialize<RecordData>(text);
                if (data != null)
                {
                    if (!data.Type.Equals("random_seed")) continue;
                    if (data.Stats == null || data.Stats.Count == 0) continue;

                    if (data.FinalRTA == 0)
                    {
                        ResetData.WallResets += 1;
                        continue;
                    }

                    //TODO: 0 Need to make reading fast reseting not in wall




                    datas.Add(data);
                }
            }
            catch (JsonException ex)
            {
                StartViewModel.Log($"Error deserializing {records[i]}: {ex.Message}", ConsoleLineOption.Error);
            }
        }

        return datas;

    }

    private void ResetStatsData()
    {
        ResetData.Reset();
    }
    private void ClearResetFolder()
    {
        /*try
        {
            if (Directory.Exists(_recordsFolder))
            {
                string[] entries = Directory.GetFileSystemEntries(_recordsFolder);

                if (entries.Length != 0)
                {
                    Directory.Delete(_recordsFolder, true);
                    Directory.CreateDirectory(_recordsFolder);
                    StartViewModel.Log($"Cleared folder: {_recordsFolder}");
                }
            }
            else
                _recordsFolder = string.Empty;
        }
        catch (Exception)
        {
            StartViewModel.Log($"Error with clearing {_recordsFolder}", ConsoleLineOption.Error);
        }*/
    }
}
