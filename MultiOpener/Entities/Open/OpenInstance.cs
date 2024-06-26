﻿using MultiOpener.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using System;
using System.Windows;
using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels;
using MultiOpener.Commands.StartCommands;

namespace MultiOpener.Entities.Open;

public class OpenInstance : OpenItem
{
    public int DelayBetweenInstances { get; set; }

    public int Quantity { get; set; }
    public ObservableCollection<string> Names { get; set; }

    public bool ShowNamesInsteadOfTitle { get; set; }

    public string OfflineModeName { get; set; }
    public bool OfflineMode { get; set; }


    [JsonConstructor]
    public OpenInstance(string Name = "", string PathExe = "", int DelayBefore = 0, int DelayAfter = 0, OpenType Type = OpenType.InstancesMultiMC, int Quantity = 0, ObservableCollection<string>? Names = null, int DelayBetweenInstances = 0, bool ShowNamesInsteadOfTitle = false, bool OfflineMode = false, string OfflineModeName = "") : base(Name, PathExe, DelayBefore, DelayAfter, Type)
    {
        this.Quantity = Quantity;
        if (Names == null)
            this.Names = new ObservableCollection<string>();
        else
            this.Names = Names;
        this.DelayBetweenInstances = DelayBetweenInstances;
        this.ShowNamesInsteadOfTitle = ShowNamesInsteadOfTitle;
        this.OfflineMode = OfflineMode;
        this.OfflineModeName = OfflineModeName;
    }
    public OpenInstance(string Name) : this(Name, "") { }
    public OpenInstance(OpenInstance instance) : base(instance)
    {
        Quantity = instance.Quantity;
        if (Names == null)
            Names = new ObservableCollection<string>();
        else
            Names = instance.Names;
        DelayBetweenInstances = instance.DelayBetweenInstances;
        ShowNamesInsteadOfTitle = instance.ShowNamesInsteadOfTitle;
        OfflineMode = instance.OfflineMode;
        OfflineModeName = instance.OfflineModeName;
    }

    public override string Validate()
    {
        string output = base.Validate();

        if (!Path.GetFileName(PathExe).Equals("MultiMC.exe"))
            return $"You set wrong path to MultiMC exe file in {Name}";

        if (!File.Exists(PathExe))
            return $"Your path to MultiMC doesn't exist in {Name}";

        if (DelayBetweenInstances > 99999 || DelayBetweenInstances < 0)
            return $"Delay between openning instances should be between 0 and 99999 in {Name}";

        if (Quantity > 32 || Quantity < 0)
            return $"Amount of instances should be between 0 and 32 in {Name}";

        int amount = 0;
        for (int i = 0; i < Names.Count; i++)
        {
            var current = Names[i];
            if (string.IsNullOrEmpty(current))
                amount++;
        }

        if (amount > 0)
            return $"You didn't set names for {amount} instances in {Name}";

        string instancePath = Path.GetDirectoryName(PathExe) + "\\instances\\";

        for (int i = 0; i < Quantity; i++)
        {
            var current = Names[i];

            if (!Directory.Exists(instancePath + current))
                return $"Instance \"{current}\" doesn't exist in multiMC instance folder";
        }

        return output;
    }

    public override async Task Open(StartViewModel startModel, CancellationToken token)
    {
        List<OpenedInstanceProcess> mcInstances = new();
        int openedCount = 0;
        ((StartOpenCommand)startModel.OpenCommand).UpdateCountForInstances();

        try
        {
            if (!token.IsCancellationRequested) await Task.Delay(DelayBefore);

            for (int i = 0; i < Quantity; i++)
            {
                bool isCancelled = token.IsCancellationRequested;
                if (!isCancelled) await Task.Delay(i == 0 ? 0 : i == 1 ? 5000 : DelayBetweenInstances < 500 ? 500 : DelayBetweenInstances);

                startModel.SetDetailedLoadingText($"({i + 1}/{Quantity}) Instance");
                ((StartOpenCommand)startModel.OpenCommand).UpdateProgressBar();

                ProcessStartInfo startInfo = new(PathExe) { UseShellExecute = false, Arguments = $"--launch \"{Names[i]}\"" };
                if (OfflineMode) startInfo.Arguments = $"--launch \"{Names[i]}\" -o -n \"{OfflineModeName}\"";

                OpenedInstanceProcess opened = new();
                string path = Path.Combine(Path.GetDirectoryName(PathExe)!, "instances", Names[i]).Replace(Path.DirectorySeparatorChar, '/');
                opened.Initialize(startInfo, Names[i], path);
                opened.Number = (short)(i + 1);
                opened.showNamesInsteadOfTitle = ShowNamesInsteadOfTitle;

                if (!isCancelled)
                {
                    openedCount++;
                    opened.FindProcess();

                    if (!opened.IsOpenedFromStatus())
                        Process.Start(startInfo);
                }
                mcInstances.Add(opened);
            }

            Regex mcPatternRegex = OpenedInstanceProcess.MCPattern();
            List<nint> instances = new();
            byte instanceCount = 0;

            if (!token.IsCancellationRequested)
            {
                startModel!.SetDetailedLoadingText($"Loading Datas");

                int errorCount = -1;
                var config = new TimeoutConfigurator(App.Config.TimeoutLookingForInstancesData, 30);
                do
                {
                    errorCount++;
                    instanceCount = Win32.GetWindowsCountByTitlePattern(mcPatternRegex);
                    try
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(config.Cooldown), token);
                    }
                    catch { break; }
                } while (instanceCount < openedCount && errorCount < config.ErrorCount);

                instances = Win32.GetWindowsByTitlePattern(mcPatternRegex);
                try
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(1000), token);
                }
                catch { }
            }

            for (int i = 0; i < Quantity; i++)
            {
                var current = mcInstances[i];
                if (current.IsOpenedFromStatus()) continue;
                if (!current.FindInstance(instances)) current.Clear();
            }
            Application.Current?.Dispatcher.Invoke(delegate { StartViewModel.Instance?.AddOpened(mcInstances); });

            if (!token.IsCancellationRequested)
            {
                startModel!.SetDetailedLoadingText($"Finalizing Datas");
                await Task.Delay(DelayAfter + App.Config.TimeoutInstanceFinalizingData);
            }
        }
        catch (Exception e)
        {
            StartViewModel.Log(e.ToString(), ConsoleLineOption.Error);
        }
    }

    public override ushort GetAdditionalProgressCount() => (ushort)(Quantity - 1);
}
