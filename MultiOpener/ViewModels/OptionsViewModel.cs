using MultiOpener.Commands.OptionsCommands;
using MultiOpener.Entities.Options;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.ViewModels
{
    public class OptionsViewModel : BaseViewModel
    {
        private int _timeLateRefresh;
        public int TimeLateRefresh
        {
            get => _timeLateRefresh;
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
            get => _timeoutOpen;
            set
            {
                _timeoutOpen = value;
                App.Config.TimeoutOpen = value;
                OnPropertyChanged(nameof(TimeoutOpen));
            }
        }

        private int _timeoutLookingForInstancesData;
        public int TimeoutLookingForInstancesData
        {
            get => _timeoutLookingForInstancesData;
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
            get => _timeoutInstanceFinalizingData;
            set
            {
                _timeoutInstanceFinalizingData = value;
                App.Config.TimeoutInstanceFinalizingData = value;
                OnPropertyChanged(nameof(TimeoutInstanceFinalizingData));
            }
        }

        private int _timeoutWaitingForSingleInstanceToOpen;
        public int TimeoutWaitingForSingleInstanceToOpen
        {
            get => _timeoutWaitingForSingleInstanceToOpen;
            set
            {
                _timeoutWaitingForSingleInstanceToOpen = value;
                App.Config.TimeoutWaitingForSingleInstanceToOpen = value;
                OnPropertyChanged(nameof(TimeoutWaitingForSingleInstanceToOpen));
            }
        }

        private int _updateResetTrackerFrequency;
        public int UpdateResetTrackerFrequency
        {
            get => _updateResetTrackerFrequency;
            set
            {
                if (value < 10000)
                    value = 10000;
                _updateResetTrackerFrequency = value;
                App.Config.UpdateResetTrackerFrequency = value;
                OnPropertyChanged(nameof(UpdateResetTrackerFrequency));
            }
        }

        private bool _deleteAllRecordOnActivating;
        public bool DeleteAllRecordOnActivating
        {
            get => _deleteAllRecordOnActivating;
            set
            {
                _deleteAllRecordOnActivating = value;
                App.Config.DeleteAllRecordOnActivating = value;
                OnPropertyChanged(nameof(DeleteAllRecordOnActivating));
            }
        }

        private bool _alwaysOnTop;
        public bool AlwaysOnTop
        {
            get => _alwaysOnTop;
            set
            {
                _alwaysOnTop = value;
                App.Config.AlwaysOnTop = value;
                Application.Current.MainWindow.Topmost = value;
                OnPropertyChanged(nameof(AlwaysOnTop));
            }
        }

        private bool _isMinimizedAfterOpen;
        public bool IsMinimizedAfterOpen
        {
            get => _isMinimizedAfterOpen;
            set
            {
                _isMinimizedAfterOpen = value;
                App.Config.IsMinimizedAfterOpen = value;
                OnPropertyChanged(nameof(IsMinimizedAfterOpen));
            }
        }

        public ICommand ResetToDefaultCommand { get; set; }

        private const string _optionsSaveFileName = "Options.json";


        public OptionsViewModel()
        {
            ResetToDefaultCommand = new OptionsResetToDefaultCommand(this);
            App.Config.ResetToDefault();
            LoadOptions();

            Application.Current.MainWindow.Topmost = App.Config.AlwaysOnTop;
        }

        public override void OnEnable() { }
        public override void OnDisable()
        {
            SaveOptions();
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
                SaveOptions();

            string text = File.ReadAllText(fileToLoad) ?? "";
            if (string.IsNullOrEmpty(text))
                return;

            var data = JsonSerializer.Deserialize<OptionSaveItem>(text);
            if (data is { })
            {
                App.Config = data;
                App.Config.UpdateUIFromConfig(this);
            }
        }

        public void ResetToDefault()
        {
            App.Config.ResetToDefault();
            App.Config.UpdateUIFromConfig(this);
        }
    }
}
