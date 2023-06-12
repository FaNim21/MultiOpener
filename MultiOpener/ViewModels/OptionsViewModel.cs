using MultiOpener.Items.Options;
using System.IO;
using System.Text.Json;

namespace MultiOpener.ViewModels
{
    /// <summary>
    /// MAIN IDEA IS TO MAKE IT SCROLLABLE?? WITH ALL SUPPORTED PROGRAMS/MECHANICS TO CONFIGURE
    /// </summary>
    public class OptionsViewModel : BaseViewModel
    {
        public OptionSaveItem config;

        private readonly string _optionsSaveFileName = "Options.json";

        private int _timeLookingForInstancesData;
        public int TimeLookingForInstancesData
        {
            get { return _timeLookingForInstancesData; }
            set
            {
                _timeLookingForInstancesData = value;
                config.TimeLookingForInstancesData = value;
                OnPropertyChanged(nameof(TimeLookingForInstancesData));
            }
        }

        public OptionsViewModel()
        {
            config = new OptionSaveItem();
            config.ResetToDefault();

            LoadOptions();
        }

        public void SaveOptions()
        {
            JsonSerializerOptions options = new() { WriteIndented = true, };
            var data = JsonSerializer.Serialize(config, options);
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
                config = data;
                UpdateUIFromConfig();
            }
        }

        private void UpdateUIFromConfig()
        {
            TimeLookingForInstancesData = config.TimeLookingForInstancesData;
        }
    }
}
