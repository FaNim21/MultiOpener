using System.Windows.Media;

namespace MultiOpener.Entities;

public enum ConsoleLineOption
{
    Normal,
    Error,
    Warning,
}

public readonly struct ConsoleLine
{
    public readonly string Text { get; init; }
    public readonly Brush Color { get; init; }
}
