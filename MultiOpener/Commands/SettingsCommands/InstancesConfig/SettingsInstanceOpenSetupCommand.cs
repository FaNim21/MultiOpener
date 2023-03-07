using MultiOpener.ViewModels.Settings;
using MultiOpener.Windows;
using System.Windows;

namespace MultiOpener.Commands.SettingsCommands.InstancesConfig
{
    public class SettingsInstanceOpenSetupCommand : BaseCommand
    {
        public SettingsOpenInstancesModelView settingsOpenInstancesModelView;

        public SettingsInstanceOpenSetupCommand(SettingsOpenInstancesModelView settingsOpenInstancesModelView) {
            this.settingsOpenInstancesModelView = settingsOpenInstancesModelView;
        }

        public override void Execute(object? parameter)
        {
            if (settingsOpenInstancesModelView.Quantity <= 0)
                return;

            MainWindow mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.Hide();

            double leftPosition = mainWindow.Left + (mainWindow.Width / 2);
            double topPosition = mainWindow.Top + (mainWindow.Height / 2);

            WindowOpenInstancesSetup instanceSetup = new(settingsOpenInstancesModelView, leftPosition, topPosition);
            instanceSetup.Show();
        }
    }
}
