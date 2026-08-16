using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain;

namespace ProjectCore.Template
{
    public interface ICheatService
    {
        public event Action CommandsChanged;

        public IReadOnlyList<CheatCommandDescriptor> Commands { get; }

        public UniTask<Result<CheatExecutionData>> ExecuteAsync(
            string input,
            CancellationToken cancellationToken);
    }
}
