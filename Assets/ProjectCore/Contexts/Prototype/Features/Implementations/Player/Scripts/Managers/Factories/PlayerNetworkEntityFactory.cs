using System;
using Domain;
using Fusion;
using ProjectCore.GameCore;

namespace ProjectCore.Prototype
{
    public sealed class PlayerNetworkEntityFactory :
        BaseNetworkEntityFactory<PlayerSpawnPayload>
    {
        private readonly NetworkEntityIdFactory _networkEntityIdFactory;

        public PlayerNetworkEntityFactory(NetworkEntityIdFactory networkEntityIdFactory)
        {
            _networkEntityIdFactory = networkEntityIdFactory;
        }

        public override NetworkEntityTypeData EntityType => PlayerNetworkEntityConstants.Player;

        public override BaseNetworkEntityRoot Spawn(
            NetworkRunner runner,
            NetworkPrefabRef prefab,
            PlayerRef? inputAuthority,
            PlayerSpawnPayload payload)
        {
            NetworkEntityIdData entityId = _networkEntityIdFactory.Create(EntityType);

            NetworkObject networkObject = runner.Spawn(
                prefab,
                payload.Position,
                payload.Rotation,
                inputAuthority,
                (_, spawnedNetworkObject) =>
                {
                    if (spawnedNetworkObject.TryGetComponent(out PlayerNetworkEntityComponent playerNetworkEntity))
                    {
                        playerNetworkEntity.SetEntityId(entityId);
                    }
                });

            if (!networkObject.TryGetComponent(out PlayerNetworkEntityComponent spawnedPlayer))
            {
                throw new InvalidOperationException($"Spawned player prefab does not contain '{nameof(PlayerNetworkEntityComponent)}'.");
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
