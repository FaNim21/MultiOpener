using MultiOpener.ViewModels;
using System.Windows;

namespace MultiOpener.Commands.SettingsCommands
{
    class SettingsLoadChosenPresetCommand : SettingsCommandBase
    {
        public SettingsLoadChosenPresetCommand(SettingsViewModel Settings) : base(Settings)
        {
        }

        public override void Execute(object? parameter)
        {
            //TODO: Zbieranie nazwy pliku do odpalenia z comboboxa i ladowanie calej listy
            MessageBox.Show("Ladowanie presetu");

            //ladowanie PresetName na koncu
        }
    }
}
