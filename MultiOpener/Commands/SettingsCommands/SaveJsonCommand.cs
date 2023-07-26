using MultiOpener.Components.Controls;
using MultiOpener.Utils;
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

            Settings.SaveCurrentOpenCommand.Execute(null);

            string saveName = Helper.GetFileNameWithoutExtension(Settings.SaveNameField);
            if (string.IsNullOrEmpty(saveName))
                return;

            var files = Directory.GetFiles(Settings.directoryPath, "*.json", SearchOption.TopDirectoryOnly);

            if (saveName != Settings.PresetName)
            {
                foreach (var item in files)
                {
                    string finalName = Helper.GetFileNameWithoutExtension(item);
                    if (finalName.ToLower().Equals(saveName.ToLower()))
                        if (DialogBox.Show($"Are you sure that you wanna overwrite {finalName}?", $"Overwriting warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
                            return;
                }
            }
            if (!string.IsNullOrEmpty(Settings.PresetName) && !Settings.PresetName.Equals(Settings.SaveNameField))
            {
                if (!saveName.Equals(Settings.PresetName, System.StringComparison.CurrentCultureIgnoreCase))
                {
                    MessageBoxResult result = DialogBox.Show($"Do you want save your preset to new file?\nYes - It will create new file with your preset or overwrite the existing one\nNo - It will change name of your current preset and file", $"Detected different names!", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.Yes) { }
                    else if (result == MessageBoxResult.No)
                    {
                        string oldName = Settings.PresetName;
                        File.Delete(Settings.directoryPath + "\\" + oldName + ".json");
                    }
                    else return;
                }
            }
            Settings.PresetName = saveName;

            JsonSerializerOptions options = new() { WriteIndented = true, };
            var data = JsonSerializer.Serialize<object>(Settings.Opens, options);
            File.WriteAllText(Settings.directoryPath + "\\" + saveName + ".json", data);

            Settings.UpdatePresetsComboBox(saveName + ".json");
            Settings.IsCurrentPresetSaved = true;
        }
    }
}
