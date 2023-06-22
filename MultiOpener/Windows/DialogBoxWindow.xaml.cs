using MultiOpener.ViewModels.DialogBox;
using System;
using System.Media;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.Windows;

public partial class DialogBoxWindow : Window
{
    public DialogBoxWindow()
    {
        InitializeComponent();

        Loaded += OnLoaded;
        Closed += OnClosed;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        PlayOpeningSound();
    }
    private void OnClosed(object? sender, EventArgs e)
    {
        MainWindow mainWindow = ((MainWindow)Application.Current.MainWindow);
        mainWindow.Opacity = 1f;
        mainWindow.Effect = null;
    }

    private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
            DragMove();
    }
    private void ExitButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }

    protected virtual void PlayOpeningSound()
    {
        switch (((DialogBoxViewModel)DataContext).Icon)
        {
            case MessageBoxImage.Information:
                SystemSounds.Asterisk.Play();
                break;
            case MessageBoxImage.Error:
                SystemSounds.Hand.Play();
                break;
            case MessageBoxImage.Warning:
                SystemSounds.Exclamation.Play();
                break;
        }
    }
}
