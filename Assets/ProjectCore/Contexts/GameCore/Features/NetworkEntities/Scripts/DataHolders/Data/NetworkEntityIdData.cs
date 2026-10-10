using System;

namespace ProjectCore.GameCore
{
    public readonly struct NetworkEntityIdData : IEquatable<NetworkEntityIdData>
    {
        public static readonly NetworkEntityIdData None = default;

        public NetworkEntityIdData(NetworkEntityType type, uint index)
        {
            NetworkEntityIdValidation.ValidateType(type);
            NetworkEntityIdValidation.ValidateIndex(index);

            Value = ((uint)type << NetworkEntityIdConstants.IndexBits) | index;
        }

        public NetworkEntityIdData(uint value)
        {
            NetworkEntityIdValidation.ValidateValue(value);
            Value = value;
        }

        public uint Value { get; }
        public NetworkEntityType Type => (NetworkEntityType)(Value >> NetworkEntityIdConstants.IndexBits);
        public uint Index => Value & NetworkEntityIdConstants.IndexMask;
        public bool IsValid => NetworkEntityIdValidation.IsValid(Value);

        public bool Equals(NetworkEntityIdData other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkEntityIdData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public override string ToString()
        {
            return $"{Type}:{Index}";
        }

        public static bool operator ==(NetworkEntityIdData left, NetworkEntityIdData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NetworkEntityIdData left, NetworkEntityIdData right)
        {
            return !left.Equals(right);
        }
    }
}
