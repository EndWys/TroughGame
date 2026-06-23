using System.Collections.Generic;

namespace Domain
{
    public sealed class NetworkEntityIdFactory
    {
        private readonly Dictionary<NetworkEntityType, int> _nextIndexes = new Dictionary<NetworkEntityType, int>();

        public NetworkEntityId Create(NetworkEntityType type)
        {
            _nextIndexes.TryGetValue(type, out int nextIndex);
            _nextIndexes[type] = nextIndex + 1;

            return new NetworkEntityId(type, nextIndex);
        }
    }
}
