using Shared;

namespace ProjectCore.TechnicalPrototype
{
    public static class PlayerSpawnErrors
    {
        public static Error EntitiesContainerNotConfigured()
        {
            return new Error("Player.EntitiesContainerNotConfigured", "Player entities container must be configured.");
        }

        public static Error SpawnComponentNotConfigured()
        {
            return new Error("Player.SpawnComponentNotConfigured", "PlayerFeature._spawnComponent must be configured.");
        }

        public static Error PrefabNotConfigured(string ownerName)
        {
            return new Error(
                "Player.PrefabNotConfigured",
                $"PlayerSpawnComponent '{ownerName}': _playerPrefab must be configured.");
        }

        public static Error SpawnPointsNotConfigured(string ownerName)
        {
            return new Error(
                "Player.SpawnPointsNotConfigured",
                $"PlayerSpawnComponent '{ownerName}': _spawnPoints must contain at least one spawn point.");
        }

        public static Error SpawnPointNotConfigured(string ownerName, int index)
        {
            return new Error(
                "Player.SpawnPointNotConfigured",
                $"PlayerSpawnComponent '{ownerName}': _spawnPoints[{index}] must be configured.");
        }
    }
}
