using System.IO;
using System.Text.Json;

namespace MultiOpener.ViewModels
{
    public class OptionsViewModel : BaseViewModel
    {
        private readonly string _optionsSaveFileName = "Options.json";

        private int _timeLookingForInstancesData;
        public int TimeLookingForInstancesData
        {
            get { return _timeLookingForInstancesData; }
            set
            {
                _timeLookingForInstancesData = value;
                OnPropertyChanged(nameof(TimeLookingForInstancesData));
            }
        }

        public OptionsViewModel()
        {
            LoadOptions();
        }

        private void SaveOptions()
        {
            //tu bedzie ladowanie wszystkich parametrow do oddzielnej klasy ze szczegolami
            //...

            //tu jest samo zapisywanie tej klasy do pliku
            JsonSerializerOptions options = new() { WriteIndented = true, };
            var data = JsonSerializer.Serialize<object>(/*tutaj ta klasa*/null, options);
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

            //var data = JsonSerializer.Deserialize<ObservableCollection<OpenItem>>(text);
            //Opens = new ObservableCollection<OpenItem>(data ?? new ObservableCollection<OpenItem>());
        }
    }
}
