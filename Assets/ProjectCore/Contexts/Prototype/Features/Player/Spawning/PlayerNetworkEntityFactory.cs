using System;
using Domain;
using Fusion;

namespace Prototype.Prototype
{
    public sealed class PlayerNetworkEntityFactory : BaseNetworkEntityFactory<PlayerSpawnPayload>
    {
        private readonly NetworkEntityIdFactory _networkEntityIdFactory;

        public PlayerNetworkEntityFactory(NetworkEntityIdFactory networkEntityIdFactory)
        {
            _networkEntityIdFactory = networkEntityIdFactory;
        }

        public override NetworkEntityType EntityType => PlayerNetworkEntityTypes.Player;

        public override BaseNetworkEntityRoot Spawn(PlayerSpawnPayload payload)
        {
            NetworkEntityId entityId = _networkEntityIdFactory.Create(EntityType);

            NetworkObject networkObject = payload.Runner.Spawn(
                payload.Prefab,
                payload.Position,
                payload.Rotation,
                payload.InputAuthority,
                (_, spawnedNetworkObject) =>
                {
                    if (spawnedNetworkObject.TryGetComponent(out PlayerNetworkEntity playerNetworkEntity))
                    {
                        playerNetworkEntity.SetEntityId(entityId);
                    }
                });

            if (!networkObject.TryGetComponent(out PlayerNetworkEntity spawnedPlayer))
            {
                throw new InvalidOperationException($"Spawned player prefab does not contain '{nameof(PlayerNetworkEntity)}'.");
            }

            return spawnedPlayer;
        }

        public override void Despawn(BaseNetworkEntityRoot entity)
        {
            if (entity == null || entity.Object == null || !entity.Object.IsValid)
            {
                return;
            }

            entity.Runner.Despawn(entity.Object);
        }
    }
}
