using Shared;

namespace ProjectCore.GameCore
{
    public static class MovementConfigErrors
    {
        public static Error ConfigNotConfigured(string configType)
        {
            return new Error("Movement.ConfigNotConfigured", $"{configType} must be configured.");
        }

        public static Error InvalidValue(string configType, string configName, string fieldName, bool isPositive)
        {
            string range = isPositive ? "greater than zero" : "non-negative";
            return new Error(
                "Movement.InvalidConfigValue",
                $"{configType} '{configName}': {fieldName} must be finite and {range}.");
        }
    }
}
