﻿using MultiOpener.Entities;
using MultiOpener.Entities.Open;
using MultiOpener.Utils;
using MultiOpener.ViewModels;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MultiOpener.Views;

public partial class SettingsView : UserControl
{
    public ICommand OnListItemClickCommand
    {
        get { return (ICommand)GetValue(OnListItemClickCommandProperty); }
        set { SetValue(OnListItemClickCommandProperty, value); }
    }
    public static readonly DependencyProperty OnListItemClickCommandProperty = DependencyProperty.Register("OnListItemClickCommand", typeof(ICommand), typeof(SettingsView), new PropertyMetadata(null));

    public ICommand TextBlockDragOverCommand
    {
        get { return (ICommand)GetValue(TextBlockDragOverCommandProperty); }
        set { SetValue(TextBlockDragOverCommandProperty, value); }
    }
    public static readonly DependencyProperty TextBlockDragOverCommandProperty = DependencyProperty.Register("TextBlockDragOverCommand", typeof(ICommand), typeof(SettingsView), new PropertyMetadata(null));

    private bool _isDragging;
    private object? _draggedItem;
    private LoadedGroupItem? _sourceGroup;
    private bool _isTreeViewContextMenuOpened;


    public SettingsView()
    {
        InitializeComponent();

        Application.Current?.Dispatcher.Invoke(delegate { DataContext = ((MainWindow)Application.Current.MainWindow).MainViewModel.settings; });

        treeView.PreviewMouseLeftButtonDown += TreeView_PreviewMouseLeftButtonDown;
        treeView.PreviewMouseMove += TreeView_PreviewMouseMove;
        treeView.PreviewDragOver += TreeView_DragOver;
        treeView.Drop += TreeView_Drop;
        treeView.ContextMenuOpening += TreeView_ContextMenuOpening;
        treeView.ContextMenuClosing += TreeView_ContextMenuClosing;
    }

    private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
    {
        Regex regex = RegexPatterns.NumbersPattern();
        e.Handled = regex.IsMatch(e.Text);
    }

    private void TextBlockMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed && sender is FrameworkElement frameworkElement)
            DragDrop.DoDragDrop(frameworkElement, new DataObject(DataFormats.Serializable, frameworkElement.DataContext), DragDropEffects.Move);
    }
    private void OnItemListClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListViewItem item) return;

        Consts.IsSwitchingBetweenOpensInSettings = true;
        Keyboard.Focus((IInputElement)sender);
        OnListItemClickCommand?.Execute((OpenItem)item.DataContext);
    }
    private void TextBlockDragOver(object sender, DragEventArgs e)
    {
        if (sender is not FrameworkElement element) return;

        var targetedItem = (OpenItem)element.DataContext;
        var insertedItem = (OpenItem)e.Data.GetData(DataFormats.Serializable);

        OpenItem[] elements = new OpenItem[2] { targetedItem, insertedItem };
        TextBlockDragOverCommand?.Execute(elements);
    }

    private void TreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _isTreeViewContextMenuOpened = false;
        if (e.OriginalSource is not FrameworkElement element || element.DataContext is not LoadedPresetItem preset) return;

        _isDragging = true;
        _draggedItem = preset;
        _sourceGroup = FindGroupByItem(_draggedItem);
        if (_sourceGroup == null) _isDragging = false;
    }
    private void TreeView_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isDragging || _draggedItem == null) return;

        DragDrop.DoDragDrop(treeView, _draggedItem, DragDropEffects.Move);
        _isDragging = false;
        _draggedItem = null;
        _sourceGroup = null;
    }
    private void TreeView_DragOver(object sender, DragEventArgs e)
    {
        if (_draggedItem == null) return;

        LoadedGroupItem? targetGroup = null;
        if (e.OriginalSource is FrameworkElement element)
            targetGroup = FindGroupByItem(element.DataContext);

        if (targetGroup != null && targetGroup != _sourceGroup)
            e.Effects = DragDropEffects.Move;
        else
            e.Effects = DragDropEffects.None;
    }
    private void TreeView_Drop(object sender, DragEventArgs e)
    {
        if ((_draggedItem == null && _sourceGroup == null) || _isTreeViewContextMenuOpened) return;

        LoadedGroupItem? targetGroup = null;
        if (e.OriginalSource is FrameworkElement element) targetGroup = FindGroupByItem(element.DataContext);
        if (_draggedItem is not LoadedPresetItem preset) return;

        if (targetGroup == null && e.OriginalSource is FrameworkElement element1 && element1.DataContext is SettingsViewModel _settings)
        {
            LoadedGroupItem? groupless = _settings.GetGroupByName("Groupless");
            if (groupless == null)
            {
                groupless = new LoadedGroupItem("Groupless", _settings);
                _settings.Groups!.Add(groupless);
            }
            targetGroup = groupless;
        }

        if (targetGroup == _sourceGroup) return;
        if (targetGroup!.Contains(preset.Name)) return;

        string oldPath = preset.GetPath();
        _sourceGroup!.Presets?.Remove(preset);
        targetGroup.AddPreset(preset);
        string newPath = preset.GetPath();

        File.Move(oldPath, newPath);

        SettingsViewModel settings = (SettingsViewModel)DataContext;
        if (!string.IsNullOrEmpty(settings.PresetName) && settings.PresetName.Equals(preset.Name, System.StringComparison.OrdinalIgnoreCase))
            settings.UpdateCurrentLoadedPreset(preset.GetPath());
    }
    private void TreeView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        TreeViewItem? treeViewItem = GetViewItemFromMousePosition<TreeViewItem, TreeView>(sender as TreeView, e.GetPosition(sender as IInputElement));
        if (treeViewItem == null) return;

        Keyboard.Focus(treeViewItem);
        ContextMenu? contextMenu = null;
        if (treeViewItem.DataContext is LoadedGroupItem group)
        {
            contextMenu = (ContextMenu)FindResource("GroupContextMenu");
            contextMenu.DataContext ??= DataContext;

            foreach (var item in contextMenu.Items)
                if (item is MenuItem menuItem)
                    menuItem.CommandParameter = group;
        }
        else if (treeViewItem.DataContext is LoadedPresetItem preset)
        {
            contextMenu = (ContextMenu)FindResource("PresetContextMenu");
            contextMenu.DataContext ??= DataContext;

            foreach (var item in contextMenu.Items)
                if (item is MenuItem menuItem)
                    menuItem.CommandParameter = preset;
        }

        treeViewItem.ContextMenu ??= contextMenu;
    }

    private void ListView_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        ListViewItem? listViewItem = GetViewItemFromMousePosition<ListViewItem, ListView>(sender as ListView, e.GetPosition(sender as IInputElement));
        if (listViewItem == null) return;

        var contextMenu = (ContextMenu)FindResource("ListViewContextMenu");
        contextMenu.DataContext ??= DataContext;

        var currentItem = listViewItem.DataContext;
        foreach (var item in contextMenu.Items)
            if (item is MenuItem menuItem)
                menuItem.CommandParameter = currentItem;

        listViewItem.ContextMenu ??= contextMenu;
    }

    private void TreeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
    {
        _isTreeViewContextMenuOpened = true;
    }
    private void TreeView_ContextMenuClosing(object sender, ContextMenuEventArgs e)
    {
        _isTreeViewContextMenuOpened = false;
    }

    private LoadedGroupItem? FindGroupByItem(object item)
    {
        if (item is LoadedGroupItem group) return group;
        if (item is LoadedPresetItem preset) return preset.ParentGroup;
        return null;
    }
    private MenuItem? FindMenuItemByName(ItemsControl itemsControl, string menuItemName)
    {
        foreach (var item in itemsControl.Items)
        {
            if (item is MenuItem menuItem && menuItem.Name == menuItemName) return menuItem;
            else if (item is ItemsControl subMenu)
            {
                var foundMenuItem = FindMenuItemByName(subMenu, menuItemName);
                if (foundMenuItem != null) return foundMenuItem;
            }
        }
        return null;
    }

    private T? GetViewItemFromMousePosition<T, U>(U? view, Point mousePosition) where T : Control where U : ItemsControl
    {
        HitTestResult hitTestResult = VisualTreeHelper.HitTest(view, mousePosition);
        DependencyObject? target = hitTestResult?.VisualHit;

        while (target != null && target is not T)
            target = VisualTreeHelper.GetParent(target);

        return target as T;
    }
}
