using MultiOpener.Entities;
using MultiOpener.ViewModels;
using System.IO;

namespace MultiOpener.Commands.SettingsCommands;

class SettingsDuplicatePresetCommand : SettingsCommandBase
{
    public SettingsDuplicatePresetCommand(SettingsViewModel Settings) : base(Settings) { }

    public override void Execute(object? parameter)
    {
        if (Settings == null || parameter == null) return;

        if (parameter is not LoadedPresetItem preset) return;

        string oldPath = preset.GetPath();
        LoadedGroupItem group = preset.ParentGroup!;
        LoadedPresetItem newPreset = new(preset.Name);
        string baseName = preset.Name;
        int suffix = 1;

        while (!Settings.IsPresetNameUnique(newPreset.Name))
        {
            if (baseName.EndsWith(")"))
            {
                int openParenthesisIndex = baseName.LastIndexOf('(');
                if (openParenthesisIndex > 0)
                {
                    int closeParenthesisIndex = baseName.LastIndexOf(')');
                    if (closeParenthesisIndex == baseName.Length - 1)
                    {
                        string suffixStr = baseName.Substring(openParenthesisIndex + 1, closeParenthesisIndex - openParenthesisIndex - 1);
                        if (int.TryParse(suffixStr, out int existingSuffix))
                        {
                            suffix = existingSuffix + 1;
                            baseName = baseName[..openParenthesisIndex].TrimEnd();
                        }
                    }
                }
            }

            newPreset.Name = $"{baseName} ({suffix++})";
        }
        group.AddPreset(newPreset);
        string newPath = newPreset.GetPath();

        File.Copy(oldPath, newPath);
    }
}
