using System;

namespace ProjectCore.Template
{
    [Flags]
    public enum DebugVisualizationValueDisplay
    {
        None = 0,
        HUD = 1,
        World = 2,
        HUDAndWorld = HUD | World
    }
}
