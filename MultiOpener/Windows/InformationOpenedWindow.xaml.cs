using System;
using System.Windows;

namespace MultiOpener.Windows;

public partial class InformationOpenedWindow : Window
{
    public InformationOpenedWindow()
    {
        InitializeComponent();

        Closed += OnClosed;
    }

    private void OnClosed(object? sender, EventArgs e)
    {
        MainWindow mainWindow = ((MainWindow)Application.Current.MainWindow);
        mainWindow.Opacity = 1f;
        mainWindow.Effect = null;
    }

    private void ExitButtonClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
