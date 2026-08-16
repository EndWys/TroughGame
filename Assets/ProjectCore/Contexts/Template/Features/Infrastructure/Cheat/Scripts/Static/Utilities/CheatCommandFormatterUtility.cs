using System;

namespace ProjectCore.Template
{
    public static class CheatCommandFormatterUtility
    {
        public static string QuoteArgument(string value)
        {
            value ??= string.Empty;

            if (value.Length > 0
                && value.IndexOfAny(new[] { ' ', '\t', '\r', '\n', '"', '\\' }) < 0)
            {
                return value;
            }

            return $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
        }
    }
}
