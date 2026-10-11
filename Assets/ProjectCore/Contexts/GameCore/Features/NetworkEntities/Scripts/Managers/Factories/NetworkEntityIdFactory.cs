using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public sealed class NetworkEntityIdFactory
    {
        private readonly Dictionary<NetworkEntityType, uint> _nextIndexes = new();

        public NetworkEntityIdData Create(NetworkEntityType type)
        {
            NetworkEntityIdValidation.ValidateType(type);
            _nextIndexes.TryGetValue(type, out uint nextIndex);
            NetworkEntityIdValidation.ValidateNextIndex(type, nextIndex);

            var entityId = new NetworkEntityIdData(type, nextIndex);
            _nextIndexes[type] = nextIndex + 1u;
            return entityId;
        }
    }
}
