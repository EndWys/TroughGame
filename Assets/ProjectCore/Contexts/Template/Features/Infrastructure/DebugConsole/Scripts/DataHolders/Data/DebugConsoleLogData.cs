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
            ID = id;
            Message = message;
            StackTrace = stackTrace;
            Type = type;
        }

        public int ID { get; }
        public string Message { get; }
        public string StackTrace { get; }
        public LogType Type { get; }
    }
}
