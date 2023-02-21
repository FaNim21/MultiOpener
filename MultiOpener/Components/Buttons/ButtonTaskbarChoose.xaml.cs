using System.Windows;
using System.Windows.Controls;

namespace MultiOpener.Components.Buttons
{
    public partial class ButtonTaskbarChoose : UserControl
    {
        public string ContentText
        {
            get
            {
                return (string)GetValue(ContextTextProperty);
            }
            set
            {
                SetValue(ContextTextProperty, value);
            }
        }

        //NIE DZIALA TO
        public static readonly DependencyProperty ContextTextProperty = DependencyProperty.Register("ContentText", typeof(string), typeof(ButtonTaskbarChoose), new PropertyMetadata(""));

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
