using MultiOpener.Components;
using MultiOpener.Components.Controls;
using MultiOpener.Entities;
using MultiOpener.Entities.Open;
using MultiOpener.Properties;
using MultiOpener.Utils;
using MultiOpener.Utils.Interfaces;
using MultiOpener.ViewModels;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MultiOpener;

public partial class MainWindow : Window, IClipboardService
{
    public MainViewModel MainViewModel { get; set; }


    public MainWindow()
    {
        InitializeComponent();
        InputController.Instance.Initialize();
        labelVersion.Content = Consts.Version;
        HotkeySetup();

        MainViewModel = new MainViewModel(this);
        DataContext = MainViewModel;

        if (Settings.Default.MainWindowLeft == -1 && Settings.Default.MainWindowTop == -1)
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        else
        {
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = Settings.Default.MainWindowLeft;
            Top = Settings.Default.MainWindowTop;
        }

        MainViewModel.settings.LoadStartupPreset(Settings.Default.LastOpenedPresetName);

        Task task = Task.Factory.StartNew(CheckForUpdates);
    }

    public void EnableDisableChoosenHeadButton(string option)
    {
        StartButton.IsEnabled = true;
        SettingsButton.IsEnabled = true;
        OptionsButton.IsEnabled = true;

        switch (option)
        {
            case "Start": StartButton.IsEnabled = false; break;
            case "Settings": SettingsButton.IsEnabled = false; break;
            case "Options": OptionsButton.IsEnabled = false; break;
            default: Trace.WriteLine("Nieznana opcja: " + option); break;
        }
    }

    private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left) DragMove();
    }
    private void MinimizeButtonsClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
    private void ExitButtonClick(object sender, RoutedEventArgs e)
    {
        if (!MainViewModel.start.OpenedIsEmpty())
        {
            MessageBoxResult result = DialogBox.Show($"Are you certain about closing MultiOpener?\nThere are still some processes that haven't been closed yet.", $"Closing!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
        }

        if (!MainViewModel.settings.IsCurrentPresetSaved)
        {
            MessageBoxResult result = DialogBox.Show($"Are you certain about closing MultiOpener?\nUnsaved changed in preset will be lost!.", $"Closing!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
        }

        if (MainViewModel.SelectedViewModel == MainViewModel.options)
            MainViewModel.options.SaveOptions();

        if (MainViewModel.SelectedViewModel == MainViewModel.settings)
            MainViewModel.settings.SaveGroupTree();

        Close();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        InputController.Instance.Cleanup();
        MainViewModel.start.ConsoleViewModel.ConsoleLines.Clear();
        base.OnClosing(e);
    }
    private void OnClosed(object sender, EventArgs e)
    {
        Settings.Default.MainWindowLeft = Left;
        Settings.Default.MainWindowTop = Top;
        Settings.Default.LastOpenedPresetName = MainViewModel.settings.CurrentLoadedChosenPath;

        Settings.Default.Save();
        Application.Current.Shutdown();
    }

    public void OnShow()
    {
        Show();
        if (!App.Config.AlwaysOnTop)
        {
            Topmost = true;
            Topmost = false;
        }
    }

    private async Task CheckForUpdates()
    {
        var checker = new UpdateChecker();
        bool output = false;

        try
        {
            output = await checker.CheckForUpdates();
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Failed to check for updates: {ex.Message}");
        }

        Application.Current.Dispatcher.Invoke(delegate
        {
            UpdateButton.Visibility = output ? Visibility.Visible : Visibility.Hidden;
        });
    }
    private void UpdateButtonClick(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/FaNim21/MultiOpener/releases/latest") { UseShellExecute = true });

        /*if (DialogBox.Show("A new version of MultiOpener is available\nClick yes to automaticaly update", "New Update", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
        {
            //TODO: 9 ZROBIC AUTO DOWNLOAD
            _ = new UpdateDownloadWindow();
        }*/
    }

    public void ChangePresetTitle(string name)
    {
        SettingsButton.ContentText = name;
    }

    public void HotkeySetup()
    {
        var refreshHotkey = new Hotkey
        {
            Key = Key.F5,
            ModifierKeys = ModifierKeys.None,
            Description = "Preset refreshing",
            Action = () => { MainViewModel.start.RefreshOpenedCommand.Execute(new object[] { false }); }
        };

        var openButtonPressHotkey = new Hotkey
        {
            Key = Key.F8,
            ModifierKeys = ModifierKeys.None,
            Description = "opening, canceling and closing preset button",
            Action = () => { Application.Current.Dispatcher.Invoke(delegate { MainViewModel.start.OpenCommand.Execute(null); }); }
        };

        var saveCurrentPreset = new Hotkey
        {
            Key = Key.S,
            ModifierKeys = ModifierKeys.Control,
            Description = "Saving edited preset in 'Presets' panel",
            Action = () =>
            {
                if (MainViewModel.SelectedViewModel == MainViewModel.settings)
                    MainViewModel.settings.SaveJsonCommand.Execute(null);
            }
        };

        var renameTextBox = new Hotkey
        {
            Key = Key.F2,
            ModifierKeys = ModifierKeys.None,
            Description = "Bind to trigger renaming elements",
            Action = () =>
            {
                EditableTextBlock? textBlock = Helper.GetFocusedUIElement<EditableTextBlock>();
                if (textBlock != null && textBlock.IsEditable)
                    textBlock.IsInEditMode = true;
            }
        };

        var deleteItem = new Hotkey
        {
            Key = Key.Delete,
            ModifierKeys = ModifierKeys.None,
            Description = "Removing item",
            Action = () =>
            {
                EditableTextBlock? textBlock = Helper.GetFocusedUIElement<EditableTextBlock>();
                if (textBlock == null) return;
                var item = textBlock.DataContext;

                if (item.GetType().Equals(typeof(OpenItem)))
                    MainViewModel.settings.RemoveCurrentOpenCommand.Execute(item);
                else if (item.GetType().Equals(typeof(LoadedPresetItem)))
                    MainViewModel.settings.RemovePresetCommand.Execute(item);
                else if (item.GetType().Equals(typeof(LoadedGroupItem)))
                    MainViewModel.settings.RemoveGroupCommand.Execute(item);
            }
        };

        InputController.Instance.AddHotkey(refreshHotkey);
        InputController.Instance.AddHotkey(openButtonPressHotkey);
        InputController.Instance.AddHotkey(saveCurrentPreset);
        InputController.Instance.AddHotkey(renameTextBox);
        InputController.Instance.AddHotkey(deleteItem);
    }

    public void CopyTextToClipboard(string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        Clipboard.SetText(text);
        ShowClipboardPopup();
        StartViewModel.Log($"You copied to clipboard: \"{text}\"");
    }

    private void ShowClipboardPopup()
    {
        if (popup.IsOpen)
            popup.IsOpen = false;

        popup.IsOpen = true;

        DoubleAnimation animation = new(1, 0, new Duration(TimeSpan.FromSeconds(1)));
        animation.Completed += (sender, e) =>
        {
            popup.IsOpen = false;
            popup.Opacity = 1;
        };

        popup.BeginAnimation(OpacityProperty, animation);
    }
}
