using MultiOpener.Commands.OptionsCommands;
using MultiOpener.Items.Options;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

namespace MultiOpener.ViewModels
{
    /// <summary>
    /// MAIN IDEA IS TO MAKE IT SCROLLABLE?? WITH ALL SUPPORTED PROGRAMS/MECHANICS TO CONFIGURE
    /// </summary>
    public class OptionsViewModel : BaseViewModel
    {
        private int _timeLateRefresh;
        public int TimeLateRefresh
        {
            get { return _timeLateRefresh; }
            set
            {
                _timeLateRefresh = value;
                App.config.TimeLateRefresh = value;
                OnPropertyChanged(nameof(TimeLateRefresh));
            }
        }

        private int _timeLookingForInstancesData;
        public int TimeLookingForInstancesData
        {
            get { return _timeLookingForInstancesData; }
            set
            {
                _timeLookingForInstancesData = value;
                App.config.TimeLookingForInstancesData = value;
                OnPropertyChanged(nameof(TimeLookingForInstancesData));
            }
        }

        private int _timeInstanceFinalizingData;
        public int TimeInstanceFinalizingData
        {
            get { return _timeInstanceFinalizingData; }
            set
            {
                _timeInstanceFinalizingData = value;
                App.config.TimeInstanceFinalizingData = value;
                OnPropertyChanged(nameof(TimeInstanceFinalizingData));
            }
        }

        public ICommand ResetToDefaultCommand { get; set; }

        private readonly string _optionsSaveFileName = "Options.json";

        public OptionsViewModel()
        {
            ResetToDefaultCommand = new OptionsResetToDefaultCommand(this);

            App.config.ResetToDefault();

            LoadOptions();
        }

        public void SaveOptions()
        {
            JsonSerializerOptions options = new() { WriteIndented = true, };
            var data = JsonSerializer.Serialize(App.config, options);
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
                App.config = data;
                UpdateUIFromConfig();
            }
        }

        private void UpdateUIFromConfig()
        {
            var config = App.config;
            TimeLateRefresh = config.TimeLateRefresh;
            TimeLookingForInstancesData = config.TimeLookingForInstancesData;
            TimeInstanceFinalizingData = config.TimeInstanceFinalizingData;
        }

        public void ResetToDefault()
        {
            App.config.ResetToDefault();
            UpdateUIFromConfig();
        }
    }
}
