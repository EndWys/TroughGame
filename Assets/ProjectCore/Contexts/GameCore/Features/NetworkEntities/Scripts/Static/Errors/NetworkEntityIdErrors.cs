using Shared;

namespace ProjectCore.GameCore
{
    public static class NetworkEntityIdErrors
    {
        public static Error UnsupportedType(NetworkEntityType type)
        {
            return new Error(
                "NetworkEntity.UnsupportedType",
                $"Network entity type '{type}' ({(byte)type}) is not a supported entity type.");
        }

        public static Error IndexOutOfRange(uint index)
        {
            return new Error(
                "NetworkEntity.IndexOutOfRange",
                $"Network entity index '{index}' exceeds {NetworkEntityIdConstants.MaximumIndex}.");
        }

        public static Error InvalidValue(uint value)
        {
            return new Error(
                "NetworkEntity.InvalidIdValue",
                $"Packed network entity id '{value}' has an unsupported type or a noncanonical None value.");
        }

        public static Error IdRequired()
        {
            return new Error("NetworkEntity.IdRequired", "Network entity id must be valid and cannot be None.");
        }

        public static Error IndexesExhausted(NetworkEntityType type)
        {
            return new Error(
                "NetworkEntity.IndexesExhausted",
                $"Network entity indexes for '{type}' are exhausted. Indexes cannot be reused within this world.");
        }
    }
}
