using System;

namespace MultiOpener.Utils.Attributes;

[AttributeUsage(AttributeTargets.All)]
public class OpenTypeAttribute : Attribute
{
    public Type? OpenTypeViewModel { get; set; }
    public bool AllowMultiple { get; set; }
    public Type? OpenType { get; set; }

    public OpenTypeAttribute() { }
}
