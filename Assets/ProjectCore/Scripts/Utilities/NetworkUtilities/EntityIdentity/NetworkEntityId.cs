using System;

namespace Domain
{
    public readonly struct NetworkEntityId : IEquatable<NetworkEntityId>
    {
        public static readonly NetworkEntityId None = new NetworkEntityId(new NetworkEntityType("none"), -1, true);

        public NetworkEntityId(NetworkEntityType type, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Network entity index cannot be negative.");
            }

            Type = type;
            Index = index;
        }

        private NetworkEntityId(NetworkEntityType type, int index, bool _)
        {
            Type = type;
            Index = index;
        }

        public NetworkEntityType Type { get; }
        public int Index { get; }
        public bool IsValid => Index >= 0 && !string.IsNullOrWhiteSpace(Type.Value) && Type != None.Type;

        public bool Equals(NetworkEntityId other)
        {
            return Type.Equals(other.Type) && Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkEntityId other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Type.GetHashCode() * 397) ^ Index;
            }
        }

        public override string ToString()
        {
            return $"{Type}:{Index}";
        }

        public static bool operator ==(NetworkEntityId left, NetworkEntityId right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NetworkEntityId left, NetworkEntityId right)
        {
            return !left.Equals(right);
        }
    }
}
