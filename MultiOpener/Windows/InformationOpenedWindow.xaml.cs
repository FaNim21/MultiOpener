using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace MultiOpener.Windows;

public partial class InformationOpenedWindow : Window
{
    public InformationOpenedWindow()
    {
        InitializeComponent();

        Closed += OnClosed;
    }
    ~InformationOpenedWindow()
    {
        Application.Current.Dispatcher.Invoke(delegate
        {
            Closed -= OnClosed;
        });
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
            hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
        base.OnSourceInitialized(e);
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

    private void WindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }
}
