using System;

namespace ProjectCore.GameCore
{
    public readonly struct NetworkEntityTypeData : IEquatable<NetworkEntityTypeData>
    {
        public NetworkEntityTypeData(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Network entity type cannot be null or whitespace.", nameof(value));
            }

            Value = value;
        }

        public string Value { get; }

        public bool Equals(NetworkEntityTypeData other)
        {
            return string.Equals(Value, other.Value, StringComparison.Ordinal);
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkEntityTypeData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value == null ? 0 : StringComparer.Ordinal.GetHashCode(Value);
        }

        public override string ToString()
        {
            return Value ?? string.Empty;
        }

        public static bool operator ==(NetworkEntityTypeData left, NetworkEntityTypeData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NetworkEntityTypeData left, NetworkEntityTypeData right)
        {
            return !left.Equals(right);
        }
    }
}
