using System;

namespace ProjectCore.Template
{
    public sealed class FeatureDebugLoggerService<TFeature> : IDebugLogger<TFeature>
    {
        private const string FeatureSuffix = "Feature";

        private readonly string _categoryPrefix;
        private readonly IDebugLogger _logger;

        public FeatureDebugLoggerService(IDebugLogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _categoryPrefix = $"[{GetCategoryName()}]";
        }

        public void LogMessage(string message)
        {
            _logger.LogMessage(FormatMessage(message));
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(FormatMessage(message));
        }

        public void LogError(string message)
        {
            _logger.LogError(FormatMessage(message));
        }

        public void LogException(Exception exception, string message = null)
        {
            _logger.LogException(exception, FormatMessage(message));
        }

        private string FormatMessage(string message)
        {
            return string.IsNullOrEmpty(message)
                ? _categoryPrefix
                : $"{_categoryPrefix} {message}";
        }

        private static string GetCategoryName()
        {
            string category = typeof(TFeature).Name;
            return category.EndsWith(FeatureSuffix, StringComparison.Ordinal)
                ? category.Substring(0, category.Length - FeatureSuffix.Length)
                : category;
        }
    }
}
