using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels.DialogBox;
using MultiOpener.Windows;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Effects;

namespace MultiOpener.Components.Controls;

public delegate bool ValidateInputFieldAccept(string name);

public static class DialogBox
{
    public static MessageBoxResult Show(string text, string caption = "", MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None)
    {
        var buttons = CreateButtons(button);
        DialogBoxViewModel model = new()
        {
            Text = text,
            Caption = caption,
            Buttons = buttons,
            Icon = icon,
            Result = MessageBoxResult.None,
        };
        Create<DialogBoxWindow, DialogBoxViewModel>(model);

        return model.Result;
    }

    public static string ShowInputField(string text, string caption = "", ValidateInputFieldAccept validate = null!)
    {
        InputFieldViewModel model = new(validate)
        {
            Text = text,
            Caption = caption,
            Result = MessageBoxResult.None,
        };
        Create<InputFieldWindow, InputFieldViewModel>(model);

        if (model.Result == MessageBoxResult.OK && !string.IsNullOrEmpty(model.Output))
            return model.Output;
        return string.Empty;
    }

    public static void ShowOpenedInformations(OpenedProcess opened)
    {
        Create<InformationOpenedWindow, OpenedProcess>(opened);
    }

    public static string ShowOpenFile(bool checkFileExists, bool multiSelect)
    {
        var dialog = new VistaOpenFileDialog();
        if (dialog.ShowDialog(GetActiveWindow()).GetValueOrDefault())
            if (dialog.CheckPathExists && dialog.CheckFileExists == checkFileExists && dialog.Multiselect == multiSelect)
                return dialog.FileName;
        return string.Empty;
    }

    private static void Create<T, U>(U model) where T : Window, new()
    {
        T? window = null;
        Application.Current?.Dispatcher.Invoke(delegate
        {
            Window? activeWindow = GetActiveWindow();
            window = new T()
            {
                Owner = activeWindow,
                DataContext = model,
                Topmost = true,
            };
            BlurMainWindow();
            window?.ShowDialog();
        });
    }
    private static IEnumerable<DialogBoxButton> CreateButtons(MessageBoxButton buttons, params string?[] names)
    {
        if (names == null || names.Length < 3)
        {
            string?[] defaultNames = new string?[] { null, null, null };
            if (names != null && names.Length > 0)
                Array.Copy(names, defaultNames, names.Length);

            names = defaultNames;
        }

        switch (buttons)
        {
            case MessageBoxButton.OK:
                return new DialogBoxButton[] {
                    new DialogBoxButton() { Title= names[0] ?? "Ok", Result = MessageBoxResult.OK } };
            case MessageBoxButton.OKCancel:
                return new DialogBoxButton[] {
                    new DialogBoxButton() { Title= names[0] ?? "Ok", Result = MessageBoxResult.OK },
                    new DialogBoxButton() { Title= names[1] ?? "Cancel", Result = MessageBoxResult.Cancel } };
            case MessageBoxButton.YesNo:
                return new DialogBoxButton[] {
                    new DialogBoxButton() { Title= names[0] ?? "Yes", Result = MessageBoxResult.Yes },
                    new DialogBoxButton() { Title= names[1] ?? "No", Result = MessageBoxResult.No } };
            case MessageBoxButton.YesNoCancel:
                return new DialogBoxButton[] {
                    new DialogBoxButton() { Title= names[0] ?? "Yes", Result = MessageBoxResult.Yes },
                    new DialogBoxButton() { Title= names[1] ?? "No", Result = MessageBoxResult.No },
                    new DialogBoxButton() { Title= names[2] ?? "Cancel", Result = MessageBoxResult.Cancel } };
            default:
                break;
        }

        return Enumerable.Empty<DialogBoxButton>();
    }

    private static void BlurMainWindow()
    {
        if (Application.Current?.MainWindow is MainWindow mainWindow)
        {
            mainWindow.Opacity = 0.75f;
            mainWindow.Effect = new BlurEffect();
        }
    }

    private static Window? GetActiveWindow()
    {
        Window? window = null;
        window = Application.Current?.MainWindow;
        window ??= Application.Current?.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        return window;
    }
}
