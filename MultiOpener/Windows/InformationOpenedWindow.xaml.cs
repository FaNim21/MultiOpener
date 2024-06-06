using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace MultiOpener.Windows;

public partial class InformationOpenedWindow : Window
{
    private bool isDragging;

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

    private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            isDragging = true;
            DragMove();
        }
        isDragging = false;
    }
    private void ExitButtonClick(object sender, RoutedEventArgs e)
    {
        Application.Current.MainWindow.IsEnabled = true;
        Application.Current.MainWindow.Activate();
        Close();
    }

    private void WindowKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape) Close();
    }

    private void WindowLocationChanged(object sender, EventArgs e)
    {
        if (Owner == null || !isDragging) return;

        double ownerWidth = Owner.Width;
        double ownerHeight = Owner.Height;

        double offsetX = (ownerWidth / 2) - (Width / 2);
        double offsetY = (ownerHeight / 2) - (Height / 2);

        Owner.Left = Left - offsetX;
        Owner.Top = Top - offsetY;
    }
}
