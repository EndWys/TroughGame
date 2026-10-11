using UnityEngine;

namespace ProjectCore.Template
{
    public interface ILogHandler
    {
        void HandleLog(string condition, string stackTrace, LogType type);
    }
}
