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

            var data = JsonSerializer.Serialize(Settings.Opens);
            File.WriteAllText(Settings.directoryPath, data);
        }
    }
}
