using System;

namespace ProjectCore.Template
{
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
    public sealed class CheatArgumentAttribute : Attribute
    {
        public CheatArgumentAttribute(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }
}
