using System.Windows;
using System.Windows.Controls;

namespace MultiOpener.Components.Buttons
{
    public partial class ButtonNormal : UserControl
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
        public static readonly DependencyProperty ContextTextProperty = DependencyProperty.Register("ContentText", typeof(string), typeof(ButtonNormal), new PropertyMetadata(""));

        public event RoutedEventHandler Click;

        public ButtonNormal()
        {
            InitializeComponent();
        }

        private void buttonClick(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(sender, e);
        }
    }
}
