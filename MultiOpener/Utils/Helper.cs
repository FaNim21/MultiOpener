using System.IO;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;

namespace MultiOpener.Utils;

public static class Helper
{
    /// <summary>
    /// Removing Json as extension from end of string
    /// </summary>
    /// <param name="path"></param>
    /// <returns>Name of file from path as string</returns>
    public static string GetFileNameWithoutExtension(string? path)
    {
        string output = Path.GetFileName(path) ?? "";
        if (output.EndsWith(".json"))
            output = output.Remove(output.Length - 5);

        return output;
    }

    public static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent == null) return null;

        int childCount = VisualTreeHelper.GetChildrenCount(parent);
        for (int i = 0; i < childCount; i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);

            if (child is T typedChild)
            {
                return typedChild;
            }

            T? foundChild = FindChild<T>(child);
            if (foundChild != null) return foundChild;
        }

        return null;
    }

    public static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        do
        {
            if (current is T ancestor) return ancestor;
            current = VisualTreeHelper.GetParent(current);
        } while (current != null);
        return null;
    }

    public static T? GetFocusedUIElement<T>() where T : DependencyObject
    {
        IInputElement focusedControl = Keyboard.FocusedElement;
        T? result = FindChild<T>((DependencyObject)focusedControl);
        return result;
    }
}
