namespace ProjectCore.GameCore
{
    public static class NetworkEntityIdConstants
    {
        public const int TypeBits = 6;
        public const int IndexBits = 32 - TypeBits;
        public const uint IndexMask = (1u << IndexBits) - 1u;
        public const uint MaximumIndex = IndexMask;
    }
}
