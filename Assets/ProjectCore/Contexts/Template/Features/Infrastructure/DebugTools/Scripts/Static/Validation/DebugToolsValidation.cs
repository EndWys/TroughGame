namespace ProjectCore.Template
{
    public static class DebugToolsValidation
    {
        public static bool IsValidTool(DebugToolTypes tool)
        {
            return tool is DebugToolTypes.Console
                or DebugToolTypes.Cheats
                or DebugToolTypes.Visualization;
        }
    }
}
