using MultiOpener.ViewModels.Settings;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiOpener.Windows
{
    public partial class WindowOpenInstancesSetup : Window
    {
        public SettingsOpenInstancesModelView openInstancesModelView;

        public int InstanceQuantity { get; set; }

        public ObservableCollection<TextBox> TextBoxes { get; set; } = new();

        private const int leftOffset = 0;
        private const int topOffset = 20;

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

            Left = windowLeftPosition - Width / 2;
            Top = windowTopPosition - Height / 2;

            ContentCanvas.Content = canvas;
        }

        protected override void OnClosed(EventArgs e)
        {
            Application.Current.MainWindow.Show();

            if(TextBoxes.Count == openInstancesModelView.instanceNames.Count)
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
                    Label label = new() { Content = count + ":", Width = 30, Height = 30, HorizontalContentAlignment = HorizontalAlignment.Right };
                    Canvas.SetLeft(label, leftOffset + (125 * i));
                    Canvas.SetTop(label, topOffset + (35 * j));

                    TextBox box = new() { Width = 100, Height = 25 };
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
