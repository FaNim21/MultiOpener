﻿using MultiOpener.ViewModels;
using System.Threading.Tasks;

namespace MultiOpener.Commands.StartCommands
{
    public class StartRefreshOpenedCommand : StartCommandBase
    {
        public StartRefreshOpenedCommand(StartViewModel startViewModel) : base(startViewModel)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Start == null || Start.OpenedIsEmpty() || Consts.IsStartPanelWorkingNow) return;

            Task task = Task.Run(Refresh);
        }

        public async Task Refresh()
        {
            if (Start == null) return;

            Consts.IsStartPanelWorkingNow = true;

            int length = Start.Opened.Count;
            for (int i = 0; i < length; i++)
            {
                var current = Start.Opened[i];

                Start.UpdateText($"Refreshing ({i + 1}/{length} - {current.Name})");

                if (current.isMCInstance && !current.IsOpened())
                {
                    Start.UpdateText($"Refreshing ({i + 1}/{length} - {current.Name}) - looking for window");
                    await current.SearchForMCInstance();
                }

                current.Update();
                await Task.Delay(100);
            }

            Consts.IsStartPanelWorkingNow = false;
            Start.UpdateText("");
        }
    }
}
