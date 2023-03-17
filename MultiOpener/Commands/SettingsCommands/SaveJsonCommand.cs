using MultiOpener.ViewModels;
using System.IO;
using System.Text.Json;

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

            if (string.IsNullOrEmpty(Settings.SaveNameField))
                return;

            //TODO: kod pod tym todo bedzie nalezal do spelnionych jezeli uda sie zapisac preset pod taka nazwa
            Settings.PresetName = Settings.SaveNameField;

            JsonSerializerOptions options = new() { WriteIndented = true, };
            var data = JsonSerializer.Serialize<object>(Settings.Opens, options);
            File.WriteAllText(Settings.directoryPath + "\\" + Settings.SaveNameField + ".json", data);
        }
    }
}
