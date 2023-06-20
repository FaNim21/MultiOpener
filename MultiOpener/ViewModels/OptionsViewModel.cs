using MultiOpener.Commands.OptionsCommands;
using MultiOpener.Items.Options;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

namespace MultiOpener.ViewModels
{
    public class OptionsViewModel : BaseViewModel
    {
        private int _timeLateRefresh;
        public int TimeLateRefresh
        {
            get { return _timeLateRefresh; }
            set
            {
                _timeLateRefresh = value;
                App.Config.TimeLateRefresh = value;
                OnPropertyChanged(nameof(TimeLateRefresh));
            }
        }

        private int _timeoutOpen;
        public int TimeoutOpen
        {
            get { return _timeoutOpen; }
            set
            {
                _timeoutOpen = value;
                App.Config.TimeoutOpen = value;
                OnPropertyChanged(nameof(TimeoutOpen));
            }
        }

        private int _timeoutSingleOpen;
        public int TimeoutSingleOpen
        {
            get { return _timeoutSingleOpen; }
            set
            {
                _timeoutSingleOpen = value;
                App.Config.TimeoutSingleOpen = value;
                OnPropertyChanged(nameof(TimeoutSingleOpen));
            }
        }

        private int _timeoutLookingForInstancesData;
        public int TimeoutLookingForInstancesData
        {
            get { return _timeoutLookingForInstancesData; }
            set
            {
                _timeoutLookingForInstancesData = value;
                App.Config.TimeoutLookingForInstancesData = value;
                OnPropertyChanged(nameof(TimeoutLookingForInstancesData));
            }
        }

        private int _timeoutInstanceFinalizingData;
        public int TimeoutInstanceFinalizingData
        {
            get { return _timeoutInstanceFinalizingData; }
            set
            {
                _timeoutInstanceFinalizingData = value;
                App.Config.TimeoutInstanceFinalizingData = value;
                OnPropertyChanged(nameof(TimeoutInstanceFinalizingData));
            }
        }

        private int _timeoutLookingForSingleInstanceData;
        public int TimeoutLookingForSingleInstanceData
        {
            get { return _timeoutLookingForSingleInstanceData; }
            set
            {
                _timeoutLookingForSingleInstanceData = value;
                App.Config.TimeoutLookingForSingleInstanceData = value;
                OnPropertyChanged(nameof(TimeoutLookingForSingleInstanceData));
            }
        }

        public ICommand ResetToDefaultCommand { get; set; }

        private readonly string _optionsSaveFileName = "Options.json";

        public OptionsViewModel()
        {
            ResetToDefaultCommand = new OptionsResetToDefaultCommand(this);

            App.Config.ResetToDefault();

            LoadOptions();
        }

        public void SaveOptions()
        {
            JsonSerializerOptions options = new() { WriteIndented = true, };
            var data = JsonSerializer.Serialize(App.Config, options);
            File.WriteAllText(Consts.AppdataPath + "\\" + _optionsSaveFileName, data);
        }
        private void LoadOptions()
        {
            string fileToLoad = Consts.AppdataPath + "\\" + _optionsSaveFileName;

            if (!File.Exists(fileToLoad))
                return;

            string text = File.ReadAllText(fileToLoad) ?? "";
            if (string.IsNullOrEmpty(text))
                return;

            var data = JsonSerializer.Deserialize<OptionSaveItem>(text);
            if (data is { })
            {
                App.Config = data;
                UpdateUIFromConfig();
            }
        }

        private void UpdateUIFromConfig()
        {
            var config = App.Config;
            TimeLateRefresh = config.TimeLateRefresh;
            TimeoutOpen = config.TimeoutOpen;
            TimeoutSingleOpen = config.TimeoutSingleOpen;

            TimeoutLookingForInstancesData = config.TimeoutLookingForInstancesData;
            TimeoutInstanceFinalizingData = config.TimeoutInstanceFinalizingData;
            TimeoutLookingForSingleInstanceData = config.TimeoutLookingForSingleInstanceData;
        }

        public void ResetToDefault()
        {
            App.Config.ResetToDefault();
            UpdateUIFromConfig();
        }
    }
}
