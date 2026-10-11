using System;
using Fusion;
using ProjectCore.GameCore;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerNetworkEntityFactory : BaseNetworkEntityFactory<PlayerSpawnPayload>
    {
        private readonly NetworkEntityIdFactory _networkEntityIdFactory;

        public PlayerNetworkEntityFactory(NetworkEntityIdFactory networkEntityIdFactory)
        {
            _networkEntityIdFactory = networkEntityIdFactory ??
                throw new ArgumentNullException(nameof(networkEntityIdFactory));
        }

        public override NetworkEntityType EntityType => PlayerNetworkEntityConstants.Player;

        public override BaseNetworkEntityRoot Spawn(
            NetworkRunner runner,
            NetworkPrefabRef prefab,
            PlayerRef? inputAuthority,
            PlayerSpawnPayload payload)
        {
            if (runner == null)
            {
                throw new ArgumentNullException(nameof(runner));
            }

            if (!prefab.IsValid)
            {
                throw new ArgumentException("Player prefab must be configured.", nameof(prefab));
            }

            NetworkPrefabId prefabId = runner.Prefabs.GetId((NetworkObjectGuid)prefab);
            if (!prefabId.IsValid)
            {
                throw new ArgumentException("Player prefab must be registered in Fusion.", nameof(prefab));
            }

            NetworkObject prefabObject = runner.Prefabs.Load(prefabId, isSynchronous: true);
            if (prefabObject == null || !prefabObject.TryGetComponent<PlayerNetworkEntityComponent>(out _))
            {
                throw new ArgumentException(
                    "Player prefab must contain PlayerNetworkEntityComponent.", nameof(prefab));
            }

            NetworkEntityIdData entityId = _networkEntityIdFactory.Create(EntityType);

            NetworkObject networkObject = runner.Spawn(
                prefabObject,
                payload.Position,
                payload.Rotation,
                inputAuthority,
                (_, spawnedNetworkObject) =>
                {
                    spawnedNetworkObject.GetComponent<PlayerNetworkEntityComponent>().SetEntityId(entityId);
                });

            if (!networkObject.TryGetComponent(out PlayerNetworkEntityComponent spawnedPlayer))
            {
                throw new InvalidOperationException(
                    $"Spawned player prefab does not contain " +
                    $"'{nameof(PlayerNetworkEntityComponent)}'.");
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
