using System;
using Fusion;

namespace Domain
{
    public interface INetworkEntityFactory
    {
        Type PayloadType { get; }
        NetworkEntityType EntityType { get; }

        BaseNetworkEntityRoot Spawn(
            NetworkRunner runner,
            NetworkPrefabRef prefab,
            PlayerRef? inputAuthority,
            INetworkEntitySpawnPayload payload);

        void Despawn(BaseNetworkEntityRoot entity);
    }

    public interface INetworkEntityFactory<in TPayload> : INetworkEntityFactory
        where TPayload : INetworkEntitySpawnPayload
    {
        BaseNetworkEntityRoot Spawn(
            NetworkRunner runner,
            NetworkPrefabRef prefab,
            PlayerRef? inputAuthority,
            TPayload payload);
    }
}
