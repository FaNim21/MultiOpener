using MultiOpener.Properties;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener
{
    public class OpenedProcess
    {
        public IntPtr hwnd;
        public IntPtr handle;

        public string? windowTitle;

        public bool SetHwnd()
        {
            IntPtr output = Win32.GetHwndFromHandle(handle);

            if (output != IntPtr.Zero)
            {
                hwnd = output;
                UpdateTitle();
                return true;
            }
            return false;
        }

        public void UpdateTitle()
        {
            if (hwnd == IntPtr.Zero) return;

            string title = Win32.GetWindowTitle(hwnd);

            if (!string.IsNullOrEmpty(title))
            {
                windowTitle = title;
            }
        }
    }

    public partial class MainWindow : Window
    {
        public List<OpenedProcess> opened = new();

        public MainViewModel MainViewModel { get; set; }

        public MainWindow()
        {
            InitializeComponent();
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

            MainViewModel.settings.LoadStartUPPreset(Settings.Default.LastOpenedPresetName);
        }

        public void EnableDisableChoosenHeadButton(string option)
        {
            bool result = option.Equals("Start");
            StartButton.IsEnabled = !result;
            SettingsButton.IsEnabled = result;
        }

        private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void MinimizeButtonsClick(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;
        private void ExitButtonClick(object sender, RoutedEventArgs e) => Close();

        private void OnClosed(object sender, System.EventArgs e)
        {
            Settings.Default.MainWindowLeft = Left;
            Settings.Default.MainWindowTop = Top;
            Settings.Default.LastOpenedPresetName = MainViewModel.settings.PresetName;

            Settings.Default.Save();
        }

        public void OnShow()
        {
            Show();
            Topmost = true;
            Topmost = false;
        }
    }
}
