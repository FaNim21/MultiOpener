using Microsoft.Win32;
using MultiOpener.Entities.Opened;
using MultiOpener.ViewModels.DialogBox;
using MultiOpener.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media.Effects;
using MultiOpener.ViewModels;

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

    [Obsolete]
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

    public static string ShowOpenFile()
    {
        OpenFileDialog openFileDialog = new() { Filter = "All Files (*.*)|*.*", };
        return openFileDialog.ShowDialog() == true ? openFileDialog.FileName : string.Empty;
    }

    private static void Create<T, TU>(TU model) where T : Window, new() where TU : BaseViewModel
    {
        T? window;
        Window? activeWindow;

        Application.Current?.Dispatcher.Invoke(delegate
        {
            activeWindow = GetActiveWindow();
            window = new T()
            {
                Owner = activeWindow,
                DataContext = model,
            };
            BlurMainWindow();
            window.ShowDialog();
        });
    }
    private static IEnumerable<DialogBoxButton> CreateButtons(MessageBoxButton buttons, params string?[]? names)
    {
        if (names == null || names.Length < 3)
        {
            var defaultNames = new string?[] { null, null, null };
            if (names is { Length: > 0 })
                Array.Copy(names, defaultNames, names.Length);

            names = defaultNames;
        }

        return buttons switch
        {
            MessageBoxButton.OK => new DialogBoxButton[]
            {
                new() { Title = names[0] ?? "Ok", Result = MessageBoxResult.OK }
            },
            MessageBoxButton.OKCancel => new DialogBoxButton[]
            {
                new() { Title = names[0] ?? "Ok", Result = MessageBoxResult.OK },
                new() { Title = names[1] ?? "Cancel", Result = MessageBoxResult.Cancel }
            },
            MessageBoxButton.YesNo => new DialogBoxButton[]
            {
                new() { Title = names[0] ?? "Yes", Result = MessageBoxResult.Yes },
                new() { Title = names[1] ?? "No", Result = MessageBoxResult.No }
            },
            MessageBoxButton.YesNoCancel => new DialogBoxButton[]
            {
                new() { Title = names[0] ?? "Yes", Result = MessageBoxResult.Yes },
                new() { Title = names[1] ?? "No", Result = MessageBoxResult.No },
                new() { Title = names[2] ?? "Cancel", Result = MessageBoxResult.Cancel }
            },
            _ => Enumerable.Empty<DialogBoxButton>()
        };
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
