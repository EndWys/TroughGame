using System.Collections.Generic;

namespace ProjectCore.Template
{
    public sealed class CheatCommandDescriptor
    {
        public string Name { get; init; }
        public string GroupName { get; init; }
        public IReadOnlyList<CheatArgumentDescriptor> Arguments { get; init; }
    }
}
