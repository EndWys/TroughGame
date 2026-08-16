using System;

namespace ProjectCore.Template
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class CheatCommandAttribute : Attribute
    {
        public CheatCommandAttribute(string name, string groupName = "General")
        {
            Name = name;
            GroupName = string.IsNullOrWhiteSpace(groupName) ? "General" : groupName;
        }

        public string Name { get; }
        public string GroupName { get; }
    }
}
