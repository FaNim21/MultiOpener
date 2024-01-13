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
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        if (PresentationSource.FromVisual(this) is HwndSource hwndSource)
            hwndSource.CompositionTarget.RenderMode = RenderMode.SoftwareOnly;
        base.OnSourceInitialized(e);
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
