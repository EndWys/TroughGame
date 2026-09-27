using System;
using System.Collections.Generic;
using Fusion;
using ProjectCore.GameCore;
using ProjectCore.Template;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemySpawnComponent : NetworkBehaviour
    {
        [SerializeField] private NetworkPrefabRef _enemyPrefab;
        [SerializeField] private Transform[] _spawnPoints = Array.Empty<Transform>();

        private NetworkEntitySpawner _networkEntitySpawner;
        private NetworkEntityRegistry _networkEntityRegistry;
        private IDebugLogger _logger;
        private int _nextSpawnIndex;

        [Inject]
        private void Construct(
            NetworkEntitySpawner networkEntitySpawner,
            NetworkEntityRegistry networkEntityRegistry,
            IDebugLogger logger)
        {
            _networkEntitySpawner = networkEntitySpawner ??
                throw new ArgumentNullException(nameof(networkEntitySpawner));
            _networkEntityRegistry = networkEntityRegistry ??
                throw new ArgumentNullException(nameof(networkEntityRegistry));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public void ValidateConfiguration()
        {
            if (!_enemyPrefab.IsValid)
            {
                throw new InvalidOperationException("Enemy prefab must be configured.");
            }

            if (_spawnPoints == null || _spawnPoints.Length == 0)
            {
                throw new InvalidOperationException("At least one enemy spawn point must be configured.");
            }

            foreach (Transform spawnPoint in _spawnPoints)
            {
                if (spawnPoint == null)
                {
                    throw new InvalidOperationException(
                        "Enemy spawn points must not contain null references.");
                }
            }
        }

        private void Update()
        {
            Keyboard keyboard = Keyboard.current;

            if (keyboard == null)
            {
                return;
            }

            if (keyboard.f7Key.wasPressedThisFrame)
            {
                SpawnEnemy();
            }

            if (keyboard.f8Key.wasPressedThisFrame)
            {
                DespawnEnemies();
            }
        }

        public bool SpawnEnemy()
        {
            if (!CanManageEnemies())
            {
                return false;
            }

            ValidateConfiguration();
            Transform spawnPoint = _spawnPoints[_nextSpawnIndex];
            _networkEntitySpawner.Spawn(
                Runner,
                _enemyPrefab,
                new EnemySpawnPayload(spawnPoint.position, spawnPoint.rotation));
            _nextSpawnIndex = (_nextSpawnIndex + 1) % _spawnPoints.Length;
            return true;
        }

        public int DespawnEnemies()
        {
            if (!CanManageEnemies())
            {
                return 0;
            }

            var enemies = new List<BaseNetworkEntityRoot>(
                _networkEntityRegistry.GetByType(EnemyNetworkEntityConstants.Enemy));
            int despawnedEnemiesCount = 0;

            foreach (BaseNetworkEntityRoot enemy in enemies)
            {
                if (enemy != null && enemy.Object != null && enemy.Object.IsValid && enemy.Runner == Runner)
                {
                    _networkEntitySpawner.Despawn(enemy);
                    despawnedEnemiesCount++;
                }
            }

            return despawnedEnemiesCount;
        }

        public bool CanManageEnemies()
        {
            if (_logger == null)
            {
                throw new InvalidOperationException("Enemy commands require an initialized gameplay scene.");
            }

            if (Runner != null && Runner.IsRunning && Runner.IsServer)
            {
                return true;
            }

            _logger.LogWarning("Enemy commands are only available in a running Solo/Host session.");
            return false;
        }
    }
}
