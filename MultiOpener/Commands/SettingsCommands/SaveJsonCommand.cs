using MultiOpener.ViewModels;
using System.IO;
using System.Text.Json;
using System.Windows;

namespace MultiOpener.Commands.SettingsCommands
{
    public class SaveJsonCommand : SettingsCommandBase
    {
        public SaveJsonCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            if (Settings == null) return;

            string saveName = Settings.SaveNameField ?? "";
            if (string.IsNullOrEmpty(saveName))
                return;

            var files = Directory.GetFiles(Settings.directoryPath, "*.json", SearchOption.TopDirectoryOnly);

            if (saveName != Settings.PresetName)
            {
                foreach (var item in files)
                {
                    string[] splits = item.Split('\\');
                    string finalName = splits[^1];
                    if (finalName.ToLower().Equals(saveName.ToLower() + ".json"))
                        if (MessageBox.Show($"Are you sure that you wanna overwrite {finalName}?", $"Overwriting warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                            return;
                }

                Settings.PresetName = saveName;
            }

            //TODO: Aktualizowanie comboboxa z nowym zapisanym plikiem

            JsonSerializerOptions options = new() { WriteIndented = true, };
            var data = JsonSerializer.Serialize<object>(Settings.Opens, options);
            File.WriteAllText(Settings.directoryPath + "\\" + saveName + ".json", data);
        }
    }
}
