using System;

namespace ProjectCore.Template
{
    [Flags]
    public enum DebugVisualizationValueDisplay
    {
        None = 0,
        Hud = 1,
        World = 2,
        HudAndWorld = Hud | World
    }
}
