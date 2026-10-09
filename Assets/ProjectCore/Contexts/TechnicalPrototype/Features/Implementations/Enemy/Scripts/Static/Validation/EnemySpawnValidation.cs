using System;
using Fusion;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public static class EnemySpawnValidation
    {
        public static void ValidatePrefab(NetworkPrefabRef enemyPrefab, string enemyName)
        {
            if (!enemyPrefab.IsValid)
            {
                throw new InvalidOperationException($"{enemyName} prefab must be configured.");
            }
        }

        public static void ValidateSpawnPoints(Transform[] spawnPoints)
        {
            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                throw new InvalidOperationException("At least one enemy spawn point must be configured.");
            }

            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint == null)
                {
                    throw new InvalidOperationException(
                        "Enemy spawn points must not contain null references.");
                }
            }
        }
    }
}
