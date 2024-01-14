using MultiOpener.Entities.Misc;
using System;
using System.Collections.ObjectModel;
using System.Windows;

namespace MultiOpener.ViewModels.Controls;

public class ResetStatsViewModel : BaseViewModel
{
    public ObservableCollection<TrackedRunStats> Runs { get; set; } = new();

    public bool UsingBuiltIn { get; set; }
    public long LastFileDateRead { get; set; }

    #region Times
    //OVERALL
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

    private string? _elapsedTime = "00:00:00.0";
    public string? ElapsedTime
    {
        get => _elapsedTime;
        set
        {
            if (_elapsedTime == value) return;

            _elapsedTime = value;
        }
    }

    //RTA
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

    private string? _totalRTAPlayTime = "00:00:00.0";
    public string? TotalRTAPlayTime
    {
        get => _totalRTAPlayTime;
        set
        {
            _totalRTAPlayTime = value;
        }
    }

    //WALL
    private long _wallTimeMiliseconds;
    public long WallTimeMiliseconds
    {
        get => _wallTimeMiliseconds;
        set
        {
            _wallTimeMiliseconds = value;

            TimeSpan time = TimeSpan.FromMilliseconds(_wallTimeMiliseconds);
            WallTime = string.Format("{0:D2}:{1:D2}.{2:D2}.{3:D1}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds / 100);
        }
    }

    private string? _wallTime = "00:00:00.0";
    public string? WallTime
    {
        get => _wallTime;
        set
        {
            _wallTime = value;
        }
    }

    //BREAKS
    private long _breakTimeMiliseconds;
    public long BreakTimeMiliseconds
    {
        get => _breakTimeMiliseconds;
        set
        {
            _breakTimeMiliseconds = value;

            TimeSpan time = TimeSpan.FromMilliseconds(_breakTimeMiliseconds);
            BreakTime = string.Format("{0:D2}:{1:D2}.{2:D2}.{3:D1}", time.Hours, time.Minutes, time.Seconds, time.Milliseconds / 100);
        }
    }

    private string? _breakTime = "00:00:00.0";
    public string? BreakTime
    {
        get => _breakTime;
        set
        {
            _breakTime = value;
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
        }
    }

    private int _noNetherEnterResets;
    public int NoNetherEnterResets
    {
        get { return _noNetherEnterResets; }
        set
        {
            _noNetherEnterResets = value;
        }
    }

    private int _netherWithoutPickaxeReset;
    public int NetherWithoutPickaxeReset
    {
        get { return _netherWithoutPickaxeReset; }
        set
        {
            _netherWithoutPickaxeReset = value;
        }
    }

    private int _splitlessResets;
    public int SplitlessResets
    {
        get { return _splitlessResets; }
        set
        {
            _splitlessResets = value;
        }
    }

    public int Resets => WallResets + NoNetherEnterResets + SplitlessResets;

    public int ResetsPerEnter => Resets / Math.Max(NetherEntersCount, 1);
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
        }
    }

    private long _netherEntersTime;
    public long NetherEntersTime
    {
        get { return _netherEntersTime; }
        set
        {
            _netherEntersTime = value;
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
        }
    }

    private long _firstStructureEntersTime;
    public long FirstStructureEntersTime
    {
        get { return _firstStructureEntersTime; }
        set
        {
            _firstStructureEntersTime = value;
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
        }
    }

    private long _secondStructureEntersTime;
    public long SecondStructureEntersTime
    {
        get { return _secondStructureEntersTime; }
        set
        {
            _secondStructureEntersTime = value;
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
        }
    }

    private long _netherExitEntersTime;
    public long NetherExitEntersTime
    {
        get { return _netherExitEntersTime; }
        set
        {
            _netherExitEntersTime = value;
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
        }
    }

    private long _strongholdEntersTime;
    public long StrongholdEntersTime
    {
        get { return _strongholdEntersTime; }
        set
        {
            _strongholdEntersTime = value;
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
        }
    }

    private long _endEntersTime;
    public long EndEntersTime
    {
        get { return _endEntersTime; }
        set
        {
            _endEntersTime = value;
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
    private float _realNetherPerHour;
    public float RealNetherPerHour
    {
        get => _realNetherPerHour;
        set
        {
            _realNetherPerHour = value;
        }
    }

    private float _legacyNetherPerHour;
    public float LegacyNetherPerHour
    {
        get => _legacyNetherPerHour;
        set
        {
            _legacyNetherPerHour = value;
        }
    }

    public long PostNetherTimeMiliseconds { get; set; }

    #endregion


    public void Update()
    {
        OnPropertyChanged(nameof(IronPickaxeCount));

        OnPropertyChanged(nameof(NetherEntersCount));
        OnPropertyChanged(nameof(NetherEnterAverageTime));

        OnPropertyChanged(nameof(FirstStructureEntersCount));
        OnPropertyChanged(nameof(FirstStructureEnterAverageTime));

        OnPropertyChanged(nameof(SecondStructureEntersCount));
        OnPropertyChanged(nameof(SecondStructureEnterAverageTime));

        OnPropertyChanged(nameof(NetherExitEntersCount));
        OnPropertyChanged(nameof(NetherExitEnterAverageTime));

        OnPropertyChanged(nameof(StrongholdEntersCount));
        OnPropertyChanged(nameof(StrongholdEnterAverageTime));

        OnPropertyChanged(nameof(EndEntersCount));
        OnPropertyChanged(nameof(EndEnterAverageTime));

        OnPropertyChanged(nameof(ElapsedTime));
        OnPropertyChanged(nameof(TotalRTAPlayTime));
        OnPropertyChanged(nameof(WallTime));
        OnPropertyChanged(nameof(BreakTime));

        OnPropertyChanged(nameof(Resets));
        OnPropertyChanged(nameof(NoNetherEnterResets));
        OnPropertyChanged(nameof(NetherWithoutPickaxeReset));
        OnPropertyChanged(nameof(SplitlessResets));
        OnPropertyChanged(nameof(ResetsPerEnter));

        UpdatePerHourStats();
        //...
    }

    public void AddNewRun(TrackedRunStats run)
    {
        Application.Current?.Dispatcher.Invoke(delegate
        {
            run.Count = Runs.Count + 1;
            Runs.Insert(0, run);
        });
    }

    public void UpdateTimes(long timeFromStopwatch)
    {
        ElapsedTimeMiliseconds = timeFromStopwatch;
        OnPropertyChanged(nameof(ElapsedTime));
    }
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
        float ratio = NetherEntersCount / ((ElapsedTimeMiliseconds - BreakTimeMiliseconds) / 3_600_000f);
        RealNetherPerHour = (float)Math.Round(ratio, 2);
        OnPropertyChanged(nameof(RealNetherPerHour));

        ratio = NetherEntersCount / ((ElapsedTimeMiliseconds - PostNetherTimeMiliseconds - BreakTimeMiliseconds) / 3_600_000f);
        LegacyNetherPerHour = (float)Math.Round(ratio, 2);
        OnPropertyChanged(nameof(LegacyNetherPerHour));


        //..
    }

    public void Clear()
    {
        Application.Current?.Dispatcher.Invoke(delegate
        {
            Runs.Clear();
        });

        LastFileDateRead = 0;

        ElapsedTimeMiliseconds = 0;
        TotalRTAPlayTimeMiliseconds = 0;
        WallTimeMiliseconds = 0;

        NoNetherEnterResets = 0;
        WallResets = 0;
        SplitlessResets = 0;

        RealNetherPerHour = 0;

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