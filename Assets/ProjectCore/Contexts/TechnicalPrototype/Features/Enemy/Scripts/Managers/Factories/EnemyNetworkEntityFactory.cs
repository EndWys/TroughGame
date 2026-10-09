using System;
using Fusion;
using ProjectCore.GameCore;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyNetworkEntityFactory : BaseNetworkEntityFactory<EnemySpawnPayload>
    {
        private readonly NetworkEntityIdFactory _networkEntityIdFactory;

        public EnemyNetworkEntityFactory(NetworkEntityIdFactory networkEntityIdFactory)
        {
            _networkEntityIdFactory = networkEntityIdFactory ??
                throw new ArgumentNullException(nameof(networkEntityIdFactory));
        }

        public override NetworkEntityTypeData EntityType => EnemyNetworkEntityConstants.Enemy;

        public override BaseNetworkEntityRoot Spawn(
            NetworkRunner runner,
            NetworkPrefabRef prefab,
            PlayerRef? inputAuthority,
            EnemySpawnPayload payload)
        {
            EnsureHost(runner);

            if (!prefab.IsValid)
            {
                throw new ArgumentException("Enemy prefab must be configured.", nameof(prefab));
            }

            if (inputAuthority.HasValue && inputAuthority.Value != PlayerRef.None)
            {
                throw new ArgumentException(
                    "Enemy cannot have player input authority.", nameof(inputAuthority));
            }

            NetworkPrefabId prefabId = runner.Prefabs.GetId((NetworkObjectGuid)prefab);
            if (!prefabId.IsValid)
            {
                throw new ArgumentException("Enemy prefab must be registered in Fusion.", nameof(prefab));
            }

            NetworkObject prefabObject = runner.Prefabs.Load(prefabId, isSynchronous: true);
            if (prefabObject == null || !prefabObject.TryGetComponent<EnemyNetworkEntityComponent>(out _))
            {
                throw new ArgumentException(
                    "Enemy prefab must contain EnemyNetworkEntityComponent.", nameof(prefab));
            }

            NetworkEntityIdData entityId = _networkEntityIdFactory.Create(EntityType);
            NetworkObject networkObject = runner.Spawn(
                prefabObject,
                payload.Position,
                payload.Rotation,
                inputAuthority: null,
                onBeforeSpawned: (_, spawnedNetworkObject) =>
                {
                    spawnedNetworkObject.GetComponent<EnemyNetworkEntityComponent>().SetEntityId(entityId);
                });

            return networkObject.GetComponent<EnemyNetworkEntityComponent>();
        }

        public override void Despawn(BaseNetworkEntityRoot entity)
        {
            if (entity == null || entity.Object == null || !entity.Object.IsValid)
            {
                return;
            }

            EnsureHost(entity.Runner);

            if (entity is not EnemyNetworkEntityComponent)
            {
                throw new ArgumentException("Enemy factory can only despawn Enemy entities.", nameof(entity));
            }

            entity.Runner.Despawn(entity.Object);
        }

        private static void EnsureHost(NetworkRunner runner)
        {
            if (runner == null)
            {
                throw new ArgumentNullException(nameof(runner));
            }

            if (!runner.IsRunning || !runner.IsServer)
            {
                throw new InvalidOperationException(
                    "Enemy spawn and despawn require a running Solo/Host session.");
            }
        }
    }
}
