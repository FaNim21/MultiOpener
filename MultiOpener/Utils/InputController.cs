using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.Utils;

public class InputController
{
    public static InputController Instance => instance;

    private static readonly InputController instance = new();
    private readonly HashSet<Key> pressedKeys = new();
    private readonly HashSet<Key> previousKeys = new();

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
}
