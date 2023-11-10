using MultiOpener.Components.Controls;
using MultiOpener.ViewModels;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace MultiOpener.Commands.StartCommands;

public class StartCloseCommand : StartCommandBase
{
    public bool isForcedToClose = false;

    public StartCloseCommand(StartViewModel? startViewModel) : base(startViewModel) { }

    public override void Execute(object? parameter)
    {
        if (Consts.IsStartPanelWorkingNow) return;

        var task = Task.Run(Close);
    }

    private async Task Close()
    {
        if (Start == null) return;
        if (Start.OpenedIsEmpty())
        {
            Start.SetStartButtonState(StartButtonState.open);
            return;
        }

        MessageBoxResult? result = isForcedToClose ? MessageBoxResult.Yes : DialogBox.Show("Are you sure?", "Closing your app sequence", MessageBoxButton.YesNo, MessageBoxImage.Warning);
        if (result == MessageBoxResult.Yes)
        {
            var output = "";
            for (int i = 0; i < Start.Opened.Count; i++)
            {
                var current = Start.Opened[i];
                current.Update();

                if (current.IsOpenedFromStatus())
                    output += "Closed and ";

                bool isSucceed = await current.Close();
                if (isSucceed || current.Hwnd == IntPtr.Zero || !current.IsOpenedFromStatus())
                {
                    Application.Current?.Dispatcher.Invoke(delegate
                    {
                        Start.RemoveOpened(current);
                        i--;
                    });
                    Start.LogLine($"{output}Removed {current.Name}");
                }
                await Task.Delay(5);
            }

            if (Start.OpenedIsEmpty())
            {
                Start.SetStartButtonState(StartButtonState.open);
                Consts.IsStartPanelWorkingNow = true;
            }
        }
    }
}
