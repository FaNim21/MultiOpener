using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiOpener.Components.Buttons
{
    public partial class ButtonTaskbarChoose : UserControl
    {
        public string ContentText
        {
            get { return (string)GetValue(ContextTextProperty); }
            set { SetValue(ContextTextProperty, value); }
        }

        public ICommand ChangeCommand
        {
            get { return (ICommand)GetValue(ChangeCommandProperty); }
            set { SetValue(ChangeCommandProperty, value); }
        }

        public object CommandParameter
        {
            get { return (object)GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        public static readonly DependencyProperty ContextTextProperty = DependencyProperty.Register("ContentText", typeof(string), typeof(ButtonTaskbarChoose), new PropertyMetadata(""));
        public static readonly DependencyProperty ChangeCommandProperty = DependencyProperty.Register("ChangeCommand", typeof(ICommand), typeof(ButtonTaskbarChoose), new UIPropertyMetadata(null));
        public static readonly DependencyProperty CommandParameterProperty = DependencyProperty.Register("CommandParameter", typeof(object), typeof(ButtonTaskbarChoose), new PropertyMetadata(""));

        public event RoutedEventHandler Click;

        public ButtonTaskbarChoose()
        {
            InitializeComponent();
        }

        private void buttonClick(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(sender, e);
        }
    }
}
