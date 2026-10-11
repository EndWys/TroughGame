using UnityEngine;

namespace ProjectCore.Template
{
    internal sealed class DebugConsoleLogData
    {
        public DebugConsoleLogData(
            int id,
            string message,
            string stackTrace,
            LogType type)
        {
            Id = id;
            Message = message;
            StackTrace = stackTrace;
            Type = type;
        }

        public int Id { get; }
        public string Message { get; }
        public string StackTrace { get; }
        public LogType Type { get; }
    }
}
