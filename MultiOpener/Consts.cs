﻿using System;
using System.IO;

namespace MultiOpener;

public static class Consts
{
    public const string Version = "v1.0.0-PREVIEW20";

    public static bool IsStartPanelWorkingNow { get; set; } = true;

    public static readonly string AppdataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MultiOpener");

    public static bool IsSwitchingBetweenOpensInSettings = false;
}
