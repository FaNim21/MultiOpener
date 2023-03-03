using MultiOpener.ViewModels;
using MultiOpener.Windows;
using System.Windows;

namespace MultiOpener.Commands.SettingsCommands.InstancesConfig
{
    public class SettingsInstanceOpenSetupCommand : SettingsCommandBase
    {
        public SettingsInstanceOpenSetupCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            Application.Current.MainWindow.Hide();

            WindowOpenInstancesSetup instanceSetup = new();
            instanceSetup.Show();
        }
    }
}
