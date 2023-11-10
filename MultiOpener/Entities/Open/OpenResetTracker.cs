﻿using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels;
using System.Diagnostics;
using System;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Windows;

namespace MultiOpener.Entities.Open;

public sealed class OpenResetTracker : OpenItem
{
    public string TrackerID { get; set; }
    public bool UsingBuiltInTracker { get; set; }


    [JsonConstructor]
    public OpenResetTracker(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = OpenType.ResetTrackerMC, bool MinimizeOnOpen = false, bool UsingBuiltInTracker = true, string TrackerID = "")
    {
        this.Name = Name;
        this.PathExe = PathExe;
        this.DelayBefore = DelayBefore;
        this.DelayAfter = DelayAfter;
        this.Type = Type;
        this.MinimizeOnOpen = MinimizeOnOpen;
        //...
        this.UsingBuiltInTracker = UsingBuiltInTracker;
        this.TrackerID = TrackerID;
    }
    public OpenResetTracker(string Name) : this(Name, "") { }
    public OpenResetTracker(OpenResetTracker item) : this(item.Name, item.PathExe, item.DelayBefore, item.DelayAfter, item.Type, item.MinimizeOnOpen, item.UsingBuiltInTracker, item.TrackerID) { }

    public override string Validate()
    {
        if (UsingBuiltInTracker)
        {
            if (DelayAfter < 0 || DelayBefore < 0)
                return $"You set delay lower than 0 in {Name}";

            if (DelayAfter > 999999 || DelayBefore > 99999)
                return $"Your delay can't be higher than 99999 in {Name}";
            return string.Empty;
        }

        return base.Validate();
    }

    public override async Task Open(StartViewModel startModel, CancellationToken token)
    {
        OpenedResetTrackerProcess opened = new();
        bool isCancelled = token.IsCancellationRequested;

        if (UsingBuiltInTracker)
        {
            opened.Initialize(null, "Tracker", string.Empty);
            opened.Setup(TrackerID, UsingBuiltInTracker);
            opened.isMinimizeOnOpen = MinimizeOnOpen;
            if (!isCancelled)
                opened.ActivateTracker();
        }
        else
        {
            try
            {
                if (!isCancelled) await Task.Delay(DelayBefore);

                string executable = Path.GetFileName(PathExe);
                string pathDir = Path.GetDirectoryName(PathExe) ?? "";
                ProcessStartInfo startInfo = new() { WorkingDirectory = pathDir, FileName = executable, UseShellExecute = true };
                string? name = Path.GetFileNameWithoutExtension(startInfo?.FileName);
                opened.Initialize(startInfo, name!, PathExe);
                opened.Setup(TrackerID, UsingBuiltInTracker);
                opened.isMinimizeOnOpen = MinimizeOnOpen;

                if (isCancelled) opened.Clear();
                else
                {
                    opened.FindProcess();

                    if (!opened.IsOpenedFromStatus())
                        await opened.OpenProcess(token);
                }


                if (!isCancelled) await Task.Delay(DelayAfter);
            }
            catch (Exception e)
            {
                StartViewModel.Log(e.ToString(), ConsoleLineOption.Error);
            }
        }

        Application.Current?.Dispatcher.Invoke(delegate { ((MainWindow)Application.Current.MainWindow).MainViewModel.start.AddOpened(opened); });
    }
}
