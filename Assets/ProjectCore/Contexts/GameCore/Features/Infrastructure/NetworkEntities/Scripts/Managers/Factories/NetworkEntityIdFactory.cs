using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public sealed class NetworkEntityIdFactory
    {
        private readonly Dictionary<NetworkEntityTypeData, int> _nextIndexes = new Dictionary<NetworkEntityTypeData, int>();

        public NetworkEntityIdData Create(NetworkEntityTypeData type)
        {
            _nextIndexes.TryGetValue(type, out int nextIndex);
            _nextIndexes[type] = nextIndex + 1;

            return new NetworkEntityIdData(type, nextIndex);
        }
    }
}
