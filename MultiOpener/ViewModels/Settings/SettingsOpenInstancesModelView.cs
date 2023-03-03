using MultiOpener.Commands.SettingsCommands.InstancesConfig;
using MultiOpener.ListView;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.ViewModels.Settings
{
    public class SettingsOpenInstancesModelView : OpenTypeViewModelBase
    {
        private int _quantity;
        public int Quantity
        {
            get { return _quantity; }
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public ICommand SettingsInstanceOpenSetupCommand;

        public SettingsOpenInstancesModelView()
        {
            var settings = ((MainWindow)Application.Current.MainWindow).MainViewModel.settings;
            SettingsInstanceOpenSetupCommand = new SettingsInstanceOpenSetupCommand(settings);
        }

        public override void UpdatePanelField(OpenItem currentChosen)
        {
            base.UpdatePanelField(currentChosen);

            if (currentChosen is OpenInstance instance)
            {
                Quantity = instance.Quantity;
            }
        }
    }
}
