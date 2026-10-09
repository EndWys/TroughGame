using System;
using Fusion;

namespace ProjectCore.GameCore
{
    public interface INetworkEntityFactory
    {
        Type PayloadType { get; }
        NetworkEntityTypeData EntityType { get; }

        BaseNetworkEntityRoot Spawn(
            NetworkRunner runner,
            NetworkPrefabRef prefab,
            PlayerRef? inputAuthority,
            INetworkEntitySpawnPayload payload);

        void Despawn(BaseNetworkEntityRoot entity);
    }
}
