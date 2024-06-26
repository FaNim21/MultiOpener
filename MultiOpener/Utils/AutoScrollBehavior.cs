﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MultiOpener.Utils;

public static class AutoScrollBehavior
{
    public static readonly DependencyProperty AutoScrollProperty =
        DependencyProperty.RegisterAttached("AutoScroll", typeof(bool), typeof(AutoScrollBehavior), new PropertyMetadata(false, AutoScrollPropertyChanged));

    private static bool _userScrolledUp;
    private const double _scrollingFactor = 0.01;


    private static void AutoScrollPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
    {
        bool skip = false;
        Application.Current?.Dispatcher.Invoke(delegate
        {
            var mainViewModel = ((MainWindow)Application.Current.MainWindow!).MainViewModel;

            if (mainViewModel.SelectedViewModel != mainViewModel.start)
                skip = true;
        });

        if (skip) return;
        if (obj is not ScrollViewer scrollViewer) return;

        if ((bool)args.NewValue)
        {
            scrollViewer.ScrollChanged += ScrollViewer_ScrollChanged;
            scrollViewer.PreviewMouseWheel += ScrollViewer_PreviewMouseWheel;

            if (!_userScrolledUp) scrollViewer.ScrollToEnd();
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
        if (e.VerticalChange != 0)
            _userScrolledUp = UpdateUserScrollFlag(scrollViewer!);

        if (!_userScrolledUp && (e.ExtentHeightChange > 0 || e.ViewportHeightChange > 0))
            scrollViewer?.ScrollToEnd();
    }

    private static void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer) return;

        scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - e.Delta * _scrollingFactor);
        e.Handled = true;

        _userScrolledUp = UpdateUserScrollFlag(scrollViewer!);
    }

    private static bool UpdateUserScrollFlag(ScrollViewer scrollViewer)
    {
        return scrollViewer.VerticalOffset < scrollViewer.ScrollableHeight;
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
