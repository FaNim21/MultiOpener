using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.Utils;

public class Hotkey
{
    public Key Key { get; set; }
    public ModifierKeys ModifierKeys { get; set; }
    public string? Description { get; set; }
    public Action? Action { get; set; }
}

public class InputController
{
    public static InputController Instance => instance;
    public event EventHandler? HotkeyPressed;

    private static readonly InputController instance = new();
    private readonly HashSet<Key> pressedKeys = new();
    private readonly HashSet<Key> previousKeys = new();
    private readonly List<Hotkey> hotkeys = new();


    static InputController() { }
    private InputController() { }

    public void Initialize()
    {
        Application.Current.MainWindow.KeyDown += HandleKeyDown;
        Application.Current.MainWindow.KeyUp += HandleKeyUp;
    }

    private void HandleKeyDown(object sender, KeyEventArgs e)
    {
        pressedKeys.Add(e.Key);
        CheckHotkeys();
    }
    private void HandleKeyUp(object sender, KeyEventArgs e)
    {
        pressedKeys.Remove(e.Key);
    }

    public bool GetKey(Key key)
    {
        return pressedKeys.Contains(key);
    }
    public bool GetKeyDown(Key key)
    {
        return pressedKeys.Contains(key) && !previousKeys.Contains(key);
    }

    public void UpdatePreviousKeys()
    {
        previousKeys.Clear();
        foreach (var key in pressedKeys)
            previousKeys.Add(key);
    }

    public void Cleanup()
    {
        Application.Current.MainWindow.KeyDown -= HandleKeyDown;
        Application.Current.MainWindow.KeyUp -= HandleKeyUp;
    }

    private void CheckHotkeys()
    {
        foreach (var hotkey in hotkeys)
        {
            if (IsHotkeyPressed(hotkey))
            {
                hotkey.Action?.Invoke();
                HotkeyPressed?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private bool IsHotkeyPressed(Hotkey hotkey)
    {
        return hotkey.ModifierKeys == Keyboard.Modifiers && pressedKeys.Contains(hotkey.Key);
    }

    public void AddHotkey(Hotkey hotkey)
    {
        hotkeys.Add(hotkey);
    }
    public void RemoveHotkey(Hotkey hotkey)
    {
        hotkeys.Remove(hotkey);
    }
}
