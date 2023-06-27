using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiOpener.Utils
{
    public static class AutoScrollBehavior
    {
        public static readonly DependencyProperty AutoScrollProperty =
            DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, AutoScrollPropertyChanged));

        public static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            ScrollViewer? scrollViewer = obj as ScrollViewer;
            if (scrollViewer == null) return;

            if ((bool)args.NewValue)
            {
                scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
                scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;
                if (IsScrollbarAtEnd(scrollViewer))
                {
                    scrollViewer.ScrollToEnd();
                }
            }
            else
            {
                scrollViewer.ScrollChanged -= ScrollViewer_ScrollChanged;
                scrollViewer.PreviewMouseWheel -= ScrollViewer_PreviewMouseWheel;
            }
        }

        private static void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (IsScrollbarAtEnd(scrollViewer!))
            {
                scrollViewer?.ScrollToBottom();
            }
        }

        private static void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = sender as ScrollViewer;
            if (scrollViewer != null)
            {
                double scrollingFactor = 0.01;
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta * scrollingFactor);
                e.Handled = true;
            }
        }

        private static bool IsScrollbarAtEnd(ScrollViewer scrollViewer)
        {
            double verticalOffset = scrollViewer.VerticalOffset;
            double maxVerticalOffset = scrollViewer.ExtentHeight - scrollViewer.ViewportHeight;
            return verticalOffset >= maxVerticalOffset - 1;
        }

        public static bool GetAutoScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(AutoScrollProperty);
        }

        public static void SetAutoScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(AutoScrollProperty, value);
        }
    }
}
