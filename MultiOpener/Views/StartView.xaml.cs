using MultiOpener.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace MultiOpener.Views
{
    public partial class StartView : UserControl
    {
        public StartView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            if (MessageBox.Show($"Do you want to open MultiOpener site to check for new updates or patch notes?", $"Opening Github Release site For MultiOpener", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                var processStart = new ProcessStartInfo(e.Uri.ToString())
                {
                    UseShellExecute = true,
                    Verb = "open"
                };
                Process.Start(processStart);
            }
        }


        /// <summary>
        /// TODO: -- REMOVE -- Do wywalenia po skonczeniu testowania rzeczy w hwnd
        /// </summary>
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine(((MainWindow)Application.Current.MainWindow).openedProcess.Count);



            var minecrafts = new List<WinStruct>();
            var windows = Win32.GetWindows();
            for (int i = 0; i < windows.Count; i++)
            {
                var current = windows[i];

                if (current.WinTitle.StartsWith("Minecraft"))
                {
                    minecrafts.Add(current);
                }
            }

            List<int> hwnds = new(); 
            for (int i = 0; i < ((MainWindow)Application.Current.MainWindow).openedProcess.Count; i++)
            {
                var current = ((MainWindow)Application.Current.MainWindow).openedProcess[i];

                //hwnds.Add(Win32.GetWindowHandle());
            }
        }
    }
}
