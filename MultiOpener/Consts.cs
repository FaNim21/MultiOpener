using System;
using System.IO;

namespace MultiOpener;

public static class Consts
{
    public const string Version = "v0.4.0-PREVIEW23";

    public static bool IsStartPanelWorkingNow { get; set; } = true;
    public static bool IsSwitchingBetweenOpensInSettings { get; set; } = false;

    public static readonly string AppdataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MultiOpener");
}
