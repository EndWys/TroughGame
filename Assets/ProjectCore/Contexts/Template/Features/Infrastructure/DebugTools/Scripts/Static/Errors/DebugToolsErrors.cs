using Domain;

namespace ProjectCore.Template
{
    public static class DebugToolsErrors
    {
        public static readonly Error NotInitialized = new(
            "DebugTools.NotInitialized",
            "Debug tools have not been initialized.");

        public static Error BuildNotAllowed(DebugToolTypes tool)
        {
            return new Error(
                "DebugTools.BuildNotAllowed",
                $"{tool} is not allowed in the current build.");
        }

        public static Error InvalidTool(DebugToolTypes tool)
        {
            return new Error(
                "DebugTools.InvalidTool",
                $"{tool} is not a supported debug tool.");
        }
    }
}
