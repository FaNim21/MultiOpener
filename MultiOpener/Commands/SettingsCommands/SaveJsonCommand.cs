using MultiOpener.ListView;
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

            JsonSerializerOptions options = new()
            {
                WriteIndented = true,
            };

            var data = JsonSerializer.Serialize<object>(Settings.Opens, options);
            File.WriteAllText(Settings.directoryPath, data);
        }
    }
}
