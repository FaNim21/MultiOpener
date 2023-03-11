using MultiOpener.Commands.StartCommands;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.Windows
{
    public partial class OpenningProcessLoadingWindow : Window
    {
        public StartOpenCommand StartOpenCommand { get; set; }

        public OpenningProcessLoadingWindow(StartOpenCommand openCommand, double windowLeftPosition, double windowTopPosition)
        {
            InitializeComponent();

            StartOpenCommand = openCommand;

            Left = windowLeftPosition - Width / 2;
            Top = windowTopPosition - Height / 2;
        }

        private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }

        public void SetText(string text)
        {
            InfoText.Content = text;
        }

        private void CancelButton(object sender, RoutedEventArgs e)
        {
            Button.IsEnabled = false;
            InfoText.Content = "Canceling...";

            StartOpenCommand.source.Cancel();
        }
    }
}
