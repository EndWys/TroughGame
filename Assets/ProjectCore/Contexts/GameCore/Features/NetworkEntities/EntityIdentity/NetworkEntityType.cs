using System;

namespace ProjectCore.GameCore
{
    public readonly struct NetworkEntityType : IEquatable<NetworkEntityType>
    {
        public NetworkEntityType(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Network entity type cannot be null or whitespace.", nameof(value));
            }

            Value = value;
        }

        public string Value { get; }

        public bool Equals(NetworkEntityType other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkEntityType other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool operator ==(NetworkEntityType left, NetworkEntityType right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NetworkEntityType left, NetworkEntityType right)
        {
            return !left.Equals(right);
        }
    }
}
