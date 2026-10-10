using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public static class PlayerSpawnValidation
    {
        public static void ValidateEntitiesContainer(Transform entitiesContainer)
        {
            if (entitiesContainer == null)
            {
                throw new InvalidOperationException(PlayerSpawnErrors.EntitiesContainerNotConfigured().Message);
            }
        }

        public static void ValidateConfiguration(PlayerSpawnComponent spawnComponent)
        {
            if (spawnComponent == null)
            {
                throw new InvalidOperationException(PlayerSpawnErrors.SpawnComponentNotConfigured().Message);
            }

            ValidatePrefab(spawnComponent.PlayerPrefab, spawnComponent.name);
            ValidateSpawnPoints(spawnComponent.SpawnPoints, spawnComponent.name);
        }

        public static void ValidatePrefab(NetworkPrefabRef playerPrefab, string ownerName)
        {
            if (!playerPrefab.IsValid)
            {
                throw new InvalidOperationException(PlayerSpawnErrors.PrefabNotConfigured(ownerName).Message);
            }
        }

        public static void ValidateSpawnPoints(IReadOnlyList<Transform> spawnPoints, string ownerName)
        {
            if (spawnPoints == null || spawnPoints.Count == 0)
            {
                throw new InvalidOperationException(PlayerSpawnErrors.SpawnPointsNotConfigured(ownerName).Message);
            }

            for (int index = 0; index < spawnPoints.Count; index++)
            {
                if (spawnPoints[index] == null)
                {
                    throw new InvalidOperationException(
                        PlayerSpawnErrors.SpawnPointNotConfigured(ownerName, index).Message);
                }
            }
        }
    }
}
