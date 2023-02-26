using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiOpener.Components.Buttons
{
    public partial class ButtonNormal : UserControl
    {
        public string ContentText
        {
            get { return (string)GetValue(ContextTextProperty); }
            set { SetValue(ContextTextProperty, value); }
        }

        public static readonly DependencyProperty ContextTextProperty = DependencyProperty.Register("ContentText", typeof(string), typeof(ButtonNormal), new PropertyMetadata(""));

        public ICommand OnCommand
        {
            get { return (ICommand)GetValue(OnCommandProperty); }
            set { SetValue(OnCommandProperty, value); }
        }

        public static readonly DependencyProperty OnCommandProperty = DependencyProperty.Register("OnCommand", typeof(ICommand), typeof(ButtonNormal), new PropertyMetadata(null));

        public event RoutedEventHandler? Click;


        public ButtonNormal()
        {
            InitializeComponent();
        }

        private void ButtonClick(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(sender, e);
        }
    }
}
