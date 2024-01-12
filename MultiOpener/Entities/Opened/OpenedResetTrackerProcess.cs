using MultiOpener.Commands;
using MultiOpener.ViewModels;
using MultiOpener.ViewModels.Controls;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.Entities.Opened;

public abstract class OpenedResetTrackerProcess : OpenedProcess
{
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

    public ResetStatsViewModel SessionData { get; set; } = new();

    protected CancellationTokenSource _source = new();
    protected CancellationToken _token;
    protected Task? _trackerTask;

    protected int updateFrequencySize;
    protected long uiUpdateCount = 0;


    public OpenedResetTrackerProcess()
    {
        SessionData.UsingBuiltIn = true;

        ForceUpdateCommand = new RelayCommand(ForceUpdateStats);

        //TODO: 0 Tymczasowo do statystyk sesji w main resettrackerviewmodel przed zrobieniem zapisywania runow
        Application.Current?.Dispatcher.Invoke(delegate
        {
            MainViewModel mainViewModel = ((MainWindow)Application.Current.MainWindow).MainViewModel;
            ResetTrackerViewModel? resetTrackerViewModel = mainViewModel.GetViewModel<ResetTrackerViewModel>();
            if (resetTrackerViewModel != null)
                resetTrackerViewModel.ResetTracker = this;
        });
    }

    public abstract void ActivateTracker();
    public abstract void DeactivateTracker();

    protected override void ReleaseResources()
    {
        DeactivateTracker();
    }

    private void ForceUpdateStats()
    {
        if (!IsTracking) return;

        TimeToUpdateStats = 0;
    }
}
