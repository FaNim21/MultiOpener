﻿using MultiOpener.Entities;
using MultiOpener.Entities.Interfaces;
using MultiOpener.Entities.Open;
using MultiOpener.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;

namespace MultiOpener.Components;

public partial class EditableTextBlock : UserControl
{
    public string Text
    {
        get { return (string)GetValue(TextProperty); }
        set { SetValue(TextProperty, value); }
    }
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register("Text", typeof(string), typeof(EditableTextBlock), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string PopupText
    {
        get { return (string)GetValue(PopupTextProperty); }
        set { SetValue(PopupTextProperty, value); }
    }
    public static readonly DependencyProperty PopupTextProperty = DependencyProperty.Register("PopupText", typeof(string), typeof(EditableTextBlock), new FrameworkPropertyMetadata("", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsEditable
    {
        get { return (bool)GetValue(IsEditableProperty); }
        set { SetValue(IsEditableProperty, value); }
    }
    public static readonly DependencyProperty IsEditableProperty = DependencyProperty.Register("IsEditable", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(true));

    public bool IsInEditMode
    {
        get
        {
            if (IsEditable)
                return (bool)GetValue(IsInEditModeProperty);
            else
                return false;
        }
        set
        {
            if (IsEditable)
            {
                if (value) oldText = Text;
                SetValue(IsInEditModeProperty, value);
            }
        }
    }
    public static readonly DependencyProperty IsInEditModeProperty = DependencyProperty.Register("IsInEditMode", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(false));

    private string oldText = string.Empty;
    private bool wasEnterPressed = false;


    public EditableTextBlock()
    {
        InitializeComponent();
        Focusable = true;
        FocusVisualStyle = null;
    }

    private void TextBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is not TextBox box) return;

        box.Focus();
        box.SelectAll();
    }
    private void TextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (wasEnterPressed) return;

        Text = oldText;
        IsInEditMode = false;
        wasEnterPressed = false;
    }
    private void TextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            wasEnterPressed = true;
            if (sender is TextBox textBlock)
            {
                if(textBlock.Text.Equals(oldText, StringComparison.OrdinalIgnoreCase))
                {
                    Text = textBlock.Text;
                    IsInEditMode = false;
                    e.Handled = true;
                    return;
                }

                if (SettingsViewModel.SpecialCharacterPattern().IsMatch(textBlock.Text))
                {
                    ShowPopup("The name cannot contain any of the following characters: \\ / : * ? \" < >", 3);
                    return;
                }

                bool isUnique = true;
                var settings = ((MainWindow)Application.Current.MainWindow).MainViewModel.settings;
                if (DataContext is LoadedGroupItem)
                {
                    isUnique = settings.IsGroupNameUnique(textBlock.Text);
                    if (!isUnique)
                        ShowPopup($"There already exist 'Group' item named '{textBlock.Text}'", 2);
                }
                else if (DataContext is LoadedPresetItem)
                {
                    isUnique = settings.IsPresetNameUnique(textBlock.Text);
                    if (!isUnique)
                        ShowPopup($"There already exist 'Preset' item named '{textBlock.Text}'", 2);
                }
                else if (DataContext is OpenItem)
                {
                    isUnique = settings.IsOpenNameUnique(textBlock.Text);
                    if (!isUnique)
                        ShowPopup($"There already exist 'Open' item named '{textBlock.Text}'", 2);
                }
                if (!isUnique) return;

                IRenameItem context = (IRenameItem)DataContext;
                if (context != null)
                    context.ChangeName(textBlock.Text);
                else
                    Text = textBlock.Text;

                popup.IsOpen = false;
            }

            IsInEditMode = false;
            e.Handled = true;

        }
        else if (e.Key == Key.Escape)
        {
            IsInEditMode = false;
            popup.IsOpen = false;
            e.Handled = true;
        }
    }

    private void ShowPopup(string text, int duration)
    {
        if (popup.IsOpen)
            popup.IsOpen = false;

        popup.IsOpen = true;
        PopupText = text;

        DoubleAnimation animation = new(1, 0, new Duration(TimeSpan.FromSeconds(duration)));
        animation.Completed += (sender, e) =>
        {
            popup.IsOpen = false;
            popup.Opacity = 1;
            PopupText = "";
        };

        popup.BeginAnimation(OpacityProperty, animation);
    }
}
