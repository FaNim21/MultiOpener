using System.Windows;
using System;
using System.Windows.Controls;

namespace MultiOpener.Components;

public class DynamicWrapPanel : WrapPanel
{
    //TODO: 6 Make it dynamic some day

    protected override Size MeasureOverride(Size constraint)
    {
        double currentX = 0;
        double currentY = 0;
        double panelWidth = 0;
        double panelHeight = 0;

        foreach (UIElement child in InternalChildren)
        {
            if (child.Visibility == Visibility.Visible)
            {
                child.Measure(constraint);

                if (currentX + child.DesiredSize.Width > constraint.Width)
                {
                    currentX = 0;
                    currentY += child.DesiredSize.Height;
                }

                currentX += child.DesiredSize.Width;
                panelWidth = Math.Max(panelWidth, currentX);
                panelHeight = Math.Max(panelHeight, currentY + child.DesiredSize.Height);
            }
        }

        return new Size(panelWidth, panelHeight);
    }
}