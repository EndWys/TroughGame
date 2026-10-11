using System;

namespace ProjectCore.Template
{
    public interface IDebugLogger
    {
        void LogMessage(string message);

        void LogWarning(string message);

        void LogError(string message);

        void LogException(Exception exception, string message = null);
    }
}
