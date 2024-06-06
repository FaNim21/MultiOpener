using MultiOpener.ViewModels.DialogBox;
using System.Media;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.Windows;

public partial class InputFieldWindow : Window
{
    public InputFieldWindow()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        PlayOpeningSound();

        inputField.Focus();
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
        if (DataContext is not DialogBoxViewModel dialogBox) return;

        switch (dialogBox.Icon)
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
