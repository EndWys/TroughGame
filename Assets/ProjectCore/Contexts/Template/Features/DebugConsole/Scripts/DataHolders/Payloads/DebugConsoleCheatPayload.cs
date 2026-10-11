using System.Collections.Generic;

namespace ProjectCore.Template
{
    internal sealed class DebugConsoleCheatPayload
    {
        public DebugConsoleCheatPayload(
            CheatCommandDescriptor descriptor,
            IReadOnlyList<string> argumentValues)
        {
            Descriptor = descriptor;
            ArgumentValues = argumentValues;
        }

        public CheatCommandDescriptor Descriptor { get; }
        public IReadOnlyList<string> ArgumentValues { get; }
    }
}
