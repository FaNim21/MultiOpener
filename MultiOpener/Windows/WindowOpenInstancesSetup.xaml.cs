using MultiOpener.ViewModels.Settings;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiOpener.Windows
{
    public partial class WindowOpenInstancesSetup : Window
    {
        public SettingsOpenInstancesModelView openInstancesModelView;

        public int InstanceQuantity { get; set; }

        public ObservableCollection<TextBox> TextBoxes { get; set; } = new();

        private const int leftOffset = 0;
        private const int topOffset = 20 + 30;

        public WindowOpenInstancesSetup(SettingsOpenInstancesModelView openInstancesModelView, double windowLeftPosition, double windowTopPosition)
        {
            InitializeComponent();
            this.openInstancesModelView = openInstancesModelView;
            InstanceQuantity = openInstancesModelView.Quantity;

            int columns = (int)MathF.Sqrt(InstanceQuantity);
            int rows = InstanceQuantity / columns;
            while (columns * rows < InstanceQuantity)
                rows++;

            Canvas canvas = new();
            CreateBoxes(canvas, columns, rows);

            TextBox title = new()
            {
                Background = new SolidColorBrush(Colors.Transparent),
                Text = "Folder names for minecraft instances",
                FontSize = 17,
                Foreground = new SolidColorBrush(Colors.White),
                IsReadOnly = true,
                FontFamily = new FontFamily("Dubai"),
                BorderThickness = new Thickness(0)
            };


            canvas.Children.Add(title);

            if (openInstancesModelView.instanceNames != null && openInstancesModelView.instanceNames.Any() && openInstancesModelView.instanceNames.Count > 0)
            {
                for (int i = 0; i < TextBoxes.Count; i++)
                {
                    if (i >= openInstancesModelView.instanceNames.Count)
                        break;
                    TextBoxes[i].Text = openInstancesModelView.instanceNames[i];
                }
            }

            Width = leftOffset + (125 * columns) + 5 + 30;
            Height = topOffset + (35 * (rows + 1));

            Canvas.SetLeft(title, Width / 2 - 130);
            Canvas.SetTop(title, 10);

            Left = windowLeftPosition - Width / 2;
            Top = windowTopPosition - Height / 2;

            ContentCanvas.Content = canvas;
        }

        protected override void OnClosed(EventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).OnShow();

            if (TextBoxes.Count == openInstancesModelView.instanceNames.Count)
                for (int i = 0; i < openInstancesModelView.instanceNames.Count; i++)
                    openInstancesModelView.instanceNames[i] = TextBoxes[i].Text;
            else
            {
                openInstancesModelView.instanceNames = new();
                for (int i = 0; i < TextBoxes.Count; i++)
                    openInstancesModelView.instanceNames.Add(TextBoxes[i].Text);
            }

            base.OnClosed(e);
        }

        private void CreateBoxes(Canvas canvas, int columns, int rows)
        {
            int count = 0;
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    if (count >= InstanceQuantity)
                        return;

                    count++;
                    Label label = new() { Content = count + ":", Width = 30, Height = 30, HorizontalContentAlignment = HorizontalAlignment.Right, Foreground = new SolidColorBrush(Colors.White), FontWeight = FontWeights.Bold };
                    Canvas.SetLeft(label, leftOffset + (125 * i + 3));
                    Canvas.SetTop(label, topOffset + (35 * j));

                    TextBox box = new() { Width = 100, Height = 25, Background = new SolidColorBrush(Color.FromArgb(255, 158, 158, 158)), VerticalContentAlignment = VerticalAlignment.Center };
                    Canvas.SetLeft(box, leftOffset + (125 * i + 30));
                    Canvas.SetTop(box, topOffset + (35 * j));

                    canvas.Children.Add(box);
                    canvas.Children.Add(label);
                    TextBoxes.Add(box);
                }
            }
        }

        private void HeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
        private void ExitButtonClick(object sender, RoutedEventArgs e) => Close();
    }
}
