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

            string saveName = Path.GetFileNameWithoutExtension(Settings.SaveNameField) ?? "";
            if (string.IsNullOrEmpty(saveName))
                return;

            var files = Directory.GetFiles(Settings.directoryPath, "*.json", SearchOption.TopDirectoryOnly);

            if (saveName != Settings.PresetName)
            {
                foreach (var item in files)
                {
                    string finalName = Path.GetFileNameWithoutExtension(item);
                    if (finalName.ToLower().Equals(saveName.ToLower()))
                        if (MessageBox.Show($"Are you sure that you wanna overwrite {finalName}?", $"Overwriting warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                            return;
                }
            }

            //TODO: ZTESTOWAC TO TRZEBA
            if (!string.IsNullOrEmpty(Settings.PresetName))
            {
                if (!saveName.Equals(Settings.PresetName, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    MessageBoxResult result = MessageBox.Show($"Do you want save your preset to new file?\nYes - It will create new file with your preset\nNo - It will change name of your current preset and file", $"Detected different names!", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                    if (result == MessageBoxResult.Yes)
                    {
                        //empty
                    }
                    else if (result == MessageBoxResult.No)
                    {
                        string oldName = Settings.PresetName;
                        File.Delete(Settings.directoryPath + "\\" + oldName + ".json");
                    }
                    else
                        return;
                }
            }
            Settings.PresetName = saveName;

            JsonSerializerOptions options = new() { WriteIndented = true, };
            var data = JsonSerializer.Serialize<object>(Settings.Opens, options);
            File.WriteAllText(Settings.directoryPath + "\\" + saveName + ".json", data);

            Settings.UpdatePresetsComboBox();
        }
    }
}
