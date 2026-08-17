using System;
using Domain;

namespace ProjectCore.Template
{
    public interface IDebugToolsService
    {
        event Action<DebugToolTypes, bool> ToolStateChanged;

        DebugConsoleSettings ConsoleSettings { get; }

        bool IsAllowed(DebugToolTypes tool);
        bool IsEnabled(DebugToolTypes tool);
        Result Enable(DebugToolTypes tool);
        Result Disable(DebugToolTypes tool);
    }
}
