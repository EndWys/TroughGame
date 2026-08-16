using System;
using Domain;

namespace ProjectCore.Template
{
    public static class CheatErrors
    {
        public static Error Disabled()
        {
            return new Error("Cheat.Disabled", "Cheats are disabled.");
        }

        public static Error InvalidInput(string message)
        {
            return new Error("Cheat.InvalidInput", message);
        }

        public static Error UnknownCommand(string commandName)
        {
            return new Error(
                "Cheat.UnknownCommand",
                $"Unknown cheat command '{commandName}'.");
        }

        public static Error HandlerAlreadyRegistered(Type handlerType)
        {
            return new Error(
                "Cheat.HandlerAlreadyRegistered",
                $"Cheat handler '{handlerType.Name}' is already registered.");
        }

        public static Error InvalidHandler(Type handlerType, string message)
        {
            return new Error(
                "Cheat.InvalidHandler",
                $"Cheat handler '{handlerType.Name}' is invalid: {message}");
        }

        public static Error DuplicateCommand(string commandName)
        {
            return new Error(
                "Cheat.DuplicateCommand",
                $"Cheat command '{commandName}' is already registered.");
        }

        public static Error DuplicateConverter(Type valueType)
        {
            return new Error(
                "Cheat.DuplicateConverter",
                $"A cheat argument converter for '{valueType.Name}' is already registered.");
        }

        public static Error InvalidArgumentCount(
            string commandName,
            int minimumCount,
            int maximumCount,
            int actualCount)
        {
            string expected = minimumCount == maximumCount
                ? minimumCount.ToString()
                : $"{minimumCount}-{maximumCount}";

            return new Error(
                "Cheat.InvalidArgumentCount",
                $"Command '{commandName}' expects {expected} arguments, but received {actualCount}.");
        }

        public static Error ArgumentConversionFailed(
            string argumentName,
            Type targetType,
            string rawValue)
        {
            return new Error(
                "Cheat.ArgumentConversionFailed",
                $"Cannot convert '{rawValue}' to {targetType.Name} for argument '{argumentName}'.");
        }

        public static Error ExecutionFailed(string commandName, Exception exception)
        {
            return new Error(
                "Cheat.ExecutionFailed",
                $"Command '{commandName}' failed: {exception.Message}",
                exception.ToString());
        }
    }
}
