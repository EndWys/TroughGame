namespace ProjectCore.Template
{
    internal readonly struct DebugConsoleLogFiltersData
    {
        public DebugConsoleLogFiltersData(
            bool showMessages,
            bool showWarnings,
            bool showErrors,
            bool showExceptions,
            string query)
        {
            ShowMessages = showMessages;
            ShowWarnings = showWarnings;
            ShowErrors = showErrors;
            ShowExceptions = showExceptions;
            Query = query ?? string.Empty;
        }

        public bool ShowMessages { get; }
        public bool ShowWarnings { get; }
        public bool ShowErrors { get; }
        public bool ShowExceptions { get; }
        public string Query { get; }

        public static DebugConsoleLogFiltersData Default =>
            new DebugConsoleLogFiltersData(true, true, true, true, string.Empty);
    }
}
