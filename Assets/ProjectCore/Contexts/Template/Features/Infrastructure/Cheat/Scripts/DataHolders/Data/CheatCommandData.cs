using System.Collections.Generic;

namespace ProjectCore.Template
{
    public sealed class CheatCommandData
    {
        public CheatCommandData(string name, IReadOnlyList<string> arguments)
        {
            Name = name;
            Arguments = arguments;
        }

        public string Name { get; }
        public IReadOnlyList<string> Arguments { get; }
    }
}
