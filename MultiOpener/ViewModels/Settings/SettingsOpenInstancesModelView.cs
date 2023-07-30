﻿using MultiOpener.Commands.SettingsCommands.InstancesConfig;
using MultiOpener.Entities.Open;
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
                PresetIsNotSaved();
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
                PresetIsNotSaved();
                _delayBetweenInstances = value;
                OnPropertyChanged(nameof(DelayBetweenInstances));
            }
        }

        private bool _showNamesInsteadOfTitle;
        public bool ShowNamesInsteadOfTitle
        {
            get { return _showNamesInsteadOfTitle; }
            set
            {
                PresetIsNotSaved();
                _showNamesInsteadOfTitle = value;
                OnPropertyChanged(nameof(ShowNamesInsteadOfTitle));
            }
        }

        public ICommand SettingsInstanceOpenSetupCommand { get; set; }

        public SettingsOpenInstancesModelView(SettingsViewModel settingsViewModel) : base(settingsViewModel)
        {
            SettingsInstanceOpenSetupCommand = new SettingsInstanceOpenSetupCommand(this);

            DelayBetweenInstances = 1000;
        }

        public override void UpdatePanelField(OpenItem currentChosen)
        {
            if (currentChosen is OpenInstance instance)
            {
                Quantity = instance.Quantity;
                instanceNames = instance.Names;
                DelayBetweenInstances = instance.DelayBetweenInstances;
                ShowNamesInsteadOfTitle = instance.ShowNamesInsteadOfTitle;
            }

            base.UpdatePanelField(currentChosen);
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
                openInstance.ShowNamesInsteadOfTitle = ShowNamesInsteadOfTitle;
            }
        }

        public override void Clear()
        {
            base.Clear();

            Quantity = 0;
            DelayBetweenInstances = 0;
            ShowNamesInsteadOfTitle = false;
        }
    }
}
