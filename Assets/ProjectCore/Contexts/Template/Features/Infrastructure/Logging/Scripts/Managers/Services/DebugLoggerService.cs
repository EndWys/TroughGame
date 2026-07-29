using System;
using Debug = UnityEngine.Debug;

namespace ProjectCore.Template
{
    public sealed class DebugLoggerService : IDebugLogger
    {
        public void LogMessage(string message)
        {
            Debug.Log(message ?? string.Empty);
        }

        public void LogWarning(string message)
        {
            Debug.LogWarning(message ?? string.Empty);
        }

        public void LogError(string message)
        {
            Debug.LogError(message ?? string.Empty);
        }

        public void LogException(Exception exception, string message = null)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            if (!string.IsNullOrWhiteSpace(message))
            {
                Debug.LogError(message);
            }

            Debug.LogException(exception);
        }
    }
}
