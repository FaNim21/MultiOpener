using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System.Threading.Tasks;
using System;
using System.Threading;
using System.Windows;
using System.Windows.Media;

namespace MultiOpener.Entities.Opened;

public class OpenedResetTrackerProcess : OpenedProcess
{
    public string trackerID = string.Empty;
    public bool usingBuiltInTracker = true;

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

    private CancellationTokenSource _source = new();
    private CancellationToken _token;
    private Task? _trackerTask;


    public void ActivateTracker()
    {
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
    }

    private async Task TrackStats()
    {
        while (IsTracking)
        {
            try
            {
                await Task.Delay(1000, _token);
            }
            catch { break; }

            //TODO: 0 Tracking here
            StartViewModel.Log("Tracking...");


        }
    }

    private void ResetStats()
    {
        //TODO: 0 Zrobic resetowanie wszystkich wartosci w gridzie
    }

    public override void Update(bool lookForWindow = false)
    {
        base.Update(lookForWindow);
    }

    public override void UpdateTitle()
    {
        //TODO: 0 Tu bedzie dzialac tytul w formie informowania czy to jest wbudowany tracker czy zewntetrzny i wtedy jego nazwa? :d
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

    public override Task<bool> OpenProcess(CancellationToken token = default)
    {
        if (!usingBuiltInTracker)
            return base.OpenProcess(token);

        ActivateTracker();
        return Task.FromResult(true);
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
}
