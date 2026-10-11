using System;
using System.Collections.Generic;
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

        private NetworkEntitySpawner _networkEntitySpawner;
        private int _nextSpawnIndex;

        [Inject]
        private void Construct(NetworkEntitySpawner networkEntitySpawner)
        {
            _networkEntitySpawner = networkEntitySpawner ??
                throw new ArgumentNullException(nameof(networkEntitySpawner));
        }

        public NetworkPrefabRef PlayerPrefab => _playerPrefab;

        public IReadOnlyList<Transform> SpawnPoints => _spawnPoints;

        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (!HasStateAuthority || runner.TryGetPlayerObject(player, out _))
            {
                return;
            }

            Transform spawnPoint = GetNextSpawnPoint();
            BaseNetworkEntityRoot playerEntity = _networkEntitySpawner.Spawn(
                runner,
                _playerPrefab,
                new PlayerSpawnPayload(spawnPoint.position, spawnPoint.rotation),
                player);

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
            PlayerSpawnValidation.ValidateSpawnPoints(_spawnPoints, name);

            Transform spawnPoint = _spawnPoints[_nextSpawnIndex % _spawnPoints.Length];
            _nextSpawnIndex++;

            return spawnPoint;
        }
    }
}
