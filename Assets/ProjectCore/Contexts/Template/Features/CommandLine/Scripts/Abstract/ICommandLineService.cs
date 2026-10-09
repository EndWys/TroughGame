using System;
using System.Collections.Generic;

namespace ProjectCore.Template
{
    public interface ICommandLineService
    {
        event Action CancelRequested;

        IReadOnlyList<string> PositionalArguments { get; }

        bool HasArgument(string key);
        bool HasFlag(string key);
        bool TryGetValue(string key, out string value);
        IReadOnlyList<string> GetValues(string key);
    }
}
