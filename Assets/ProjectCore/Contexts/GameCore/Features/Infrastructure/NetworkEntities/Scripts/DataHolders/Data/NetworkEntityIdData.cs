using System;

namespace ProjectCore.GameCore
{
    public readonly struct NetworkEntityIdData : IEquatable<NetworkEntityIdData>
    {
        public static readonly NetworkEntityIdData None =
            new(new NetworkEntityTypeData("none"), -1, true);

        public NetworkEntityIdData(NetworkEntityTypeData type, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index), "Network entity index cannot be negative.");
            }

            Type = type;
            Index = index;
        }

        private NetworkEntityIdData(NetworkEntityTypeData type, int index, bool _)
        {
            Type = type;
            Index = index;
        }

        public NetworkEntityTypeData Type { get; }
        public int Index { get; }
        public bool IsValid => Index >= 0 && !string.IsNullOrWhiteSpace(Type.Value) && Type != None.Type;

        public bool Equals(NetworkEntityIdData other)
        {
            return Type.Equals(other.Type) && Index == other.Index;
        }

        public override bool Equals(object obj)
        {
            return obj is NetworkEntityIdData other && Equals(other);
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
