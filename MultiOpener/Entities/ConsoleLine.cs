using System.Windows.Media;

namespace MultiOpener.Entities;

public enum ConsoleLineOption
{
    Normal,
    Error,
    Warning,
}

public class ConsoleLine
{
    public string? Text { get; set; }
    public Brush? Color { get; set; }
}
