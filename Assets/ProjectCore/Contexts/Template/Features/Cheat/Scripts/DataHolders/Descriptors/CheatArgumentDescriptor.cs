using System;

namespace ProjectCore.Template
{
    public sealed class CheatArgumentDescriptor
    {
        public string Name { get; init; }
        public Type ValueType { get; init; }
        public object DefaultValue { get; init; }
        public bool IsOptional { get; init; }
    }
}
