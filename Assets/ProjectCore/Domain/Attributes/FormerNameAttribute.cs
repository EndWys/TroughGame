using System;

namespace Domain
{
    [AttributeUsage(
        AttributeTargets.Class |
        AttributeTargets.Struct |
        AttributeTargets.Enum |
        AttributeTargets.Field,
        AllowMultiple = true,
        Inherited = true)]
    public sealed class FormerNameAttribute : Attribute
    {
        public FormerNameAttribute(string value)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public string Value { get; }
    }
}
