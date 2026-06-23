using System;

namespace Domain
{
    public interface INetworkEntityFactory
    {
        Type PayloadType { get; }
        NetworkEntityType EntityType { get; }

        BaseNetworkEntityRoot Spawn(INetworkEntitySpawnPayload payload);

        void Despawn(BaseNetworkEntityRoot entity);
    }

    public interface INetworkEntityFactory<in TPayload> : INetworkEntityFactory
        where TPayload : INetworkEntitySpawnPayload
    {
        BaseNetworkEntityRoot Spawn(TPayload payload);
    }
}
