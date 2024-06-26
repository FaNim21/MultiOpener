﻿using MultiOpener.Components;
using MultiOpener.Components.Controls;
using MultiOpener.Entities;
using MultiOpener.Entities.Open;
using MultiOpener.Properties;
using MultiOpener.Utils;
using MultiOpener.Utils.Interfaces;
using MultiOpener.ViewModels;
using MultiOpener.Windows;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media.Animation;

namespace MultiOpener;

public partial class MainWindow : Window, IClipboardService
{
    public MainViewModel MainViewModel { get; set; }

    private bool _handledCrash = false;


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

        Application.Current.Dispatcher.UnhandledException += OnDispatcherUnhandledException;

        Task.Factory.StartNew(CheckForUpdates);
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
            hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
        base.OnSourceInitialized(e);
    }

    public void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        if (_handledCrash) return;
        if (!_handledCrash) _handledCrash = true;

        StartViewModel.Log("Crash error: " + e.Exception.Message, ConsoleLineOption.Error);
        if (e.Exception.StackTrace != null)
            StartViewModel.Log("StackTrace: " + e.Exception.StackTrace, ConsoleLineOption.Error);

        MainViewModel.start.ConsoleViewModel.Save("Crash log");
    }

    private void MenuItemClick(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem clickedMenuItem) return;

        MenuItem menuItem = clickedMenuItem;
        Menu? menu = null;
        do
        {
            if (menuItem.Parent is Menu menuParent)
            {
                menu = menuParent;
                break;
            }
            menuItem = (MenuItem)menuItem.Parent;
        } while (menu == null);

        CheckMenuItems(menu!, clickedMenuItem);
    }

    private void CheckMenuItems(Menu menu, MenuItem clickedMenuItem)
    {
        foreach (MenuItem item in menu.Items)
        {
            if (item == clickedMenuItem) continue;
            item.IsChecked = false;

            CheckSubMenuItems(item, clickedMenuItem);
        }
    }
    private void CheckSubMenuItems(MenuItem menuItem, MenuItem clickedMenuItem)
    {
        foreach (MenuItem item in menuItem.Items)
        {
            if (item == clickedMenuItem) continue;

            item.IsChecked = false;
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
            var result = DialogBox.Show($"Are you certain about closing MultiOpener?\nThere are still some processes that haven't been closed yet.", $"Closing!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
        }

        if (!MainViewModel.settings.IsCurrentPresetSaved)
        {
            MessageBoxResult result = DialogBox.Show($"Are you certain about closing MultiOpener?\nUnsaved changed in preset will be lost!.", $"Closing!", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
        }

        MainViewModel.SelectedViewModel?.OnDisable();

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

        Settings.Default.Save();
        Application.Current.Shutdown();
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
            StartViewModel.Log($"Failed to check for updates: {ex.Message}", ConsoleLineOption.Error);
        }

        Application.Current.Dispatcher.Invoke(delegate
        {
            UpdateButton.Visibility = output ? Visibility.Visible : Visibility.Hidden;
        });
    }
    private void UpdateButtonClick(object sender, RoutedEventArgs e)
    {
        UpdateDownloadWindow window = new() { Owner = this };
        window.ShowDialog();
    }

    public void ChangePresetTitle(string name)
    {
        PresetsItem.Header = name;
    }

    private void HotkeySetup()
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
                var textBlock = Helper.GetFocusedUIElement<EditableTextBlock>();
                if (textBlock is { IsEditable: true })
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
                var textBlock = Helper.GetFocusedUIElement<EditableTextBlock>();
                if (textBlock == null) return;
                var item = textBlock.DataContext;

                if (item.GetType() == typeof(OpenItem))
                    MainViewModel.settings.RemoveCurrentOpenCommand.Execute(item);
                else if (item.GetType() == typeof(LoadedPresetItem))
                    MainViewModel.settings.RemovePresetCommand.Execute(item);
                else if (item.GetType() == typeof(LoadedGroupItem))
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
