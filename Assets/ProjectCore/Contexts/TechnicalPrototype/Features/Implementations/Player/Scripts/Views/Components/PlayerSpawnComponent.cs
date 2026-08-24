using System;
using Fusion;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerSpawnComponent : BaseNetworkCallbacksBehaviour
    {
        [SerializeField] private NetworkPrefabRef _playerPrefab;
        [SerializeField] private Transform[] _spawnPoints = Array.Empty<Transform>();
        [SerializeField] private Transform _entitiesContainer;

        private NetworkEntitySpawner _networkEntitySpawner;
        private int _nextSpawnIndex;

        [Inject]
        private void Construct(NetworkEntitySpawner networkEntitySpawner)
        {
            _networkEntitySpawner = networkEntitySpawner ??
                throw new ArgumentNullException(nameof(networkEntitySpawner));
        }

        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (!HasStateAuthority ||
                runner.TryGetPlayerObject(player, out _))
            {
                return;
            }

            Transform spawnPoint = GetNextSpawnPoint();
            BaseNetworkEntityRoot playerEntity = _networkEntitySpawner.Spawn(
                runner,
                _playerPrefab,
                new PlayerSpawnPayload(
                    spawnPoint.position,
                    spawnPoint.rotation),
                player);

            if (_entitiesContainer == null)
            {
                throw new InvalidOperationException(
                    "Player entities container must be configured.");
            }

            playerEntity.transform.SetParent(_entitiesContainer, true);

            runner.SetPlayerObject(player, playerEntity.Object);
        }

        public override void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
            if (!HasStateAuthority ||
                !runner.TryGetPlayerObject(player, out NetworkObject playerObject) ||
                !playerObject.TryGetComponent(out BaseNetworkEntityRoot playerEntity))
            {
                return;
            }

            _networkEntitySpawner.Despawn(playerEntity);
        }

        private Transform GetNextSpawnPoint()
        {
            if (_spawnPoints == null || _spawnPoints.Length == 0)
            {
                throw new InvalidOperationException(
                    "At least one player spawn point must be configured.");
            }

            Transform spawnPoint =
                _spawnPoints[_nextSpawnIndex % _spawnPoints.Length];
            _nextSpawnIndex++;

            return spawnPoint != null
                ? spawnPoint
                : throw new InvalidOperationException(
                    "Player spawn points must not contain null references.");
        }
    }
}
