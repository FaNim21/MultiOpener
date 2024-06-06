using MultiOpener.ViewModels;
using System.IO;
using System.Text.Json;

namespace MultiOpener.Commands.SettingsCommands;

public class SaveJsonCommand : SettingsCommandBase
{
    public SaveJsonCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null || string.IsNullOrEmpty(Settings.CurrentLoadedChosenPath)) return;

        Settings.SaveCurrentOpenCommand.Execute(null);

        JsonSerializerOptions options = new() { WriteIndented = true, };
        var data = JsonSerializer.Serialize<object>(Settings.Opens, options);
        File.WriteAllText(Settings.CurrentLoadedChosenPath, data);

        Settings.IsCurrentPresetSaved = true;
    }
}
