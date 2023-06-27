using MultiOpener.Items;
using System;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace MultiOpener.ViewModels.Controls;

public class ConsoleViewModel : BaseViewModel
{
    public ObservableCollection<ConsoleLine> ConsoleLines { get; set; }

    public ConsoleViewModel()
    {
        ConsoleLines = new ObservableCollection<ConsoleLine>();
    }

    public void ProcessCommandLine(string text, ConsoleLineOption option = ConsoleLineOption.Normal)
    {
        Application.Current?.Dispatcher.Invoke(delegate
        {
            var consoleLine = new ConsoleLine();

            StringBuilder sb = new();
            var time = DateTime.Now.ToString("HH:mm:ss");
            sb.Append("[" + time);

            if (option == ConsoleLineOption.Error)
            {
                sb.Append(" - ERROR]> ");
                consoleLine.Color = Brushes.Red;
            }
            else if (option == ConsoleLineOption.Warning)
            {
                sb.Append(" - WARNING]> ");
                consoleLine.Color = Brushes.Yellow;
            }
            else
            {
                sb.Append("]> ");
                consoleLine.Color = Brushes.White;
            }

            sb.Append(text);
            consoleLine.Text = sb.ToString();

            ConsoleLines?.Add(consoleLine);
            OnPropertyChanged(nameof(ConsoleLines));
        });
    }
}
