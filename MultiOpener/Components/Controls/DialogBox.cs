using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels.DialogBox;
using MultiOpener.Windows;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Effects;

namespace MultiOpener.Components.Controls;

public static class DialogBox
{
    //TODO: 9 Rozbudowac to o wieksza mozliwosc okreslania przyciskow + dac tu mozliwosc zmiany nazwy tych guzikow
    //TODO: 4 Dorobic tutaj wiecej rzeczy typu wybor z OctoKit sciezki do pliku exe zeby tez przyciemnialo main window

    public static MessageBoxResult Show(string text, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult result = MessageBoxResult.None)
    {
        DialogBoxViewModel model = new()
        {
            Text = text,
            Caption = caption,
            Buttons = CreateButtons(button),
            Icon = icon,
            Result = result,
        };

        Window? activeWindow = null;
        DialogBoxWindow? messageBox = null;
        Application.Current?.Dispatcher.Invoke(delegate
        {
            activeWindow = Application.Current?.MainWindow;
            activeWindow ??= Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            messageBox = new DialogBoxWindow()
            {
                Owner = activeWindow,
                DataContext = model,
                Topmost = true,
            };

            if (Application.Current?.MainWindow is MainWindow mainWindow)
            {
                mainWindow.Opacity = 0.75f;
                mainWindow.Effect = new BlurEffect();
            }

            messageBox?.ShowDialog();
        });
        return model.Result;
    }

    public static void ViewInformations(OpenedProcess opened)
    {
        Window? activeWindow = null;
        InformationOpenedWindow? window = null;
        Application.Current?.Dispatcher.Invoke(delegate
        {
            activeWindow = Application.Current?.MainWindow;
            activeWindow ??= Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);

            window = new InformationOpenedWindow()
            {
                Owner = activeWindow,
                DataContext = opened,
                Topmost = true,
            };

            if (Application.Current?.MainWindow is MainWindow mainWindow)
            {
                mainWindow.Opacity = 0.75f;
                mainWindow.Effect = new BlurEffect();
            }

            window?.ShowDialog();
        });
    }

    private static IEnumerable<DialogBoxButton> CreateButtons(MessageBoxButton buttons)
    {
        switch (buttons)
        {
            case MessageBoxButton.OK:
                return new DialogBoxButton[] {
                    new DialogBoxButton() { Text = "Ok", Result = MessageBoxResult.OK } };
            case MessageBoxButton.OKCancel:
                return new DialogBoxButton[] {
                    new DialogBoxButton() { Text = "Ok", Result = MessageBoxResult.OK },
                    new DialogBoxButton() { Text = "Cancel", Result = MessageBoxResult.Cancel } };
            case MessageBoxButton.YesNo:
                return new DialogBoxButton[] {
                    new DialogBoxButton() { Text = "Yes", Result = MessageBoxResult.Yes },
                    new DialogBoxButton() { Text = "No", Result = MessageBoxResult.No } };
            case MessageBoxButton.YesNoCancel:
                return new DialogBoxButton[] {
                    new DialogBoxButton() { Text = "Yes", Result = MessageBoxResult.Yes },
                    new DialogBoxButton() { Text = "No", Result = MessageBoxResult.No },
                    new DialogBoxButton() { Text = "Cancel", Result = MessageBoxResult.Cancel } };
            default:
                break;
        }

        return Enumerable.Empty<DialogBoxButton>();
    }
}
