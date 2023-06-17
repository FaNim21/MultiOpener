using MultiOpener.Commands.SettingsCommands.InstancesConfig;
using MultiOpener.Items;
using System;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MultiOpener.ViewModels.Settings
{
    public class SettingsOpenInstancesModelView : OpenTypeViewModelBase
    {
        public override Type ItemType { get; set; } = typeof(OpenInstance);

        public ObservableCollection<string> instanceNames = new();

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

        private int _delayBetweenInstances;
        public int DelayBetweenInstances
        {
            get { return _delayBetweenInstances; }
            set
            {
                _delayBetweenInstances = value;
                OnPropertyChanged(nameof(DelayBetweenInstances));
            }
        }

        public ICommand SettingsInstanceOpenSetupCommand { get; set; }

        public SettingsOpenInstancesModelView() : base()
        {
            SettingsInstanceOpenSetupCommand = new SettingsInstanceOpenSetupCommand(this);

            DelayBetweenInstances = 1000;
        }

        public override void UpdatePanelField(OpenItem currentChosen)
        {
            base.UpdatePanelField(currentChosen);

            if (currentChosen is OpenInstance instance)
            {
                Quantity = instance.Quantity;
                instanceNames = instance.Names;
                DelayBetweenInstances = instance.DelayBetweenInstances;
            }
        }

        public override void SetOpenProperties(ref OpenItem open)
        {
            base.SetOpenProperties(ref open);
            open.Type = OpenType.InstancesMultiMC;

            if (open is OpenInstance openInstance)
            {
                openInstance.Quantity = Quantity;
                openInstance.Names = instanceNames;
                openInstance.DelayBetweenInstances = DelayBetweenInstances;
            }
        }

        public override void Clear()
        {
            base.Clear();

            Quantity = 0;
            DelayBetweenInstances = 0;
        }
    }
}
