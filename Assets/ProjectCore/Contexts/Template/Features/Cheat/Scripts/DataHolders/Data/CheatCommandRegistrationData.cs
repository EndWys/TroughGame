using System.Reflection;

namespace ProjectCore.Template
{
    internal sealed class CheatCommandRegistrationData
    {
        public ICheatHandler Handler { get; init; }
        public MethodInfo Method { get; init; }
        public ParameterInfo[] Parameters { get; init; }
        public CheatCommandDescriptor Descriptor { get; init; }
    }
}
