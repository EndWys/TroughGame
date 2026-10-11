using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectCore.Template
{
    internal interface IDebugConsoleSystem
    {
        public event Action LogsChanged;
        public event Action CommandsChanged;

        public IReadOnlyList<DebugConsoleLogData> Logs { get; }
        public IReadOnlyList<string> CommandHistory { get; }
        public IReadOnlyList<CheatCommandDescriptor> Commands { get; }

        public void Initialize(DebugConsoleSettings settings, bool isAvailable);
        public UniTask ExecuteCommandAsync(
            string command,
            CancellationToken cancellationToken);
        public void ClearLogs();
    }
}
