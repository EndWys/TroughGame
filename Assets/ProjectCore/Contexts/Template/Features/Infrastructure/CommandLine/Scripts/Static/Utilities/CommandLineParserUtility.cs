using System;
using System.Collections.Generic;
using System.Globalization;
using Domain;

namespace ProjectCore.Template
{
    public static class CommandLineParserUtility
    {
        public static Result<CommandLineArgumentsData> Parse(
            IReadOnlyList<string> arguments)
        {
            if (arguments == null)
                throw new ArgumentNullException(nameof(arguments));

            var values = new Dictionary<string, List<string>>(StringComparer.Ordinal);
            var flags = new HashSet<string>(StringComparer.Ordinal);
            var positionalArguments = new List<string>();
            bool positionalOnly = false;

            for (var i = 0; i < arguments.Count; i++)
            {
                string argument = arguments[i] ?? string.Empty;

                if (positionalOnly)
                {
                    positionalArguments.Add(argument);
                    continue;
                }

                if (argument == "--")
                {
                    positionalOnly = true;
                    continue;
                }

                if (!IsProjectOption(argument))
                {
                    positionalArguments.Add(argument);
                    continue;
                }

                int separatorIndex = argument.IndexOf('=');
                string key = separatorIndex >= 0
                    ? argument.Substring(0, separatorIndex)
                    : argument;

                if (!CommandLineValidation.IsValidKey(key))
                {
                    return Result.Failure<CommandLineArgumentsData>(
                        CommandLineErrors.InvalidArgument(argument));
                }

                if (separatorIndex >= 0)
                {
                    AddValue(values, key, argument.Substring(separatorIndex + 1));
                    continue;
                }

                if (i + 1 < arguments.Count && CanBeValue(arguments[i + 1]))
                {
                    AddValue(values, key, arguments[++i] ?? string.Empty);
                    continue;
                }

                flags.Add(key);
            }

            return Result.Success(
                new CommandLineArgumentsData(values, flags, positionalArguments));
        }

        private static void AddValue(
            Dictionary<string, List<string>> values,
            string key,
            string value)
        {
            if (!values.TryGetValue(key, out List<string> argumentValues))
            {
                argumentValues = new List<string>();
                values.Add(key, argumentValues);
            }

            argumentValues.Add(value);
        }

        private static bool CanBeValue(string argument)
        {
            if (string.IsNullOrEmpty(argument) || !argument.StartsWith("-", StringComparison.Ordinal))
                return true;

            if (IsProjectOption(argument))
                return false;

            return double.TryParse(
                argument,
                NumberStyles.Float,
                CultureInfo.InvariantCulture,
                out _);
        }

        private static bool IsProjectOption(string argument)
        {
            return !string.IsNullOrEmpty(argument)
                   && argument.StartsWith("--", StringComparison.Ordinal);
        }
    }
}
