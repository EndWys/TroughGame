using System;
using ProjectCore.Template;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyCheatHandler : BaseCheatHandler
    {
        private EnemySpawnComponent _enemySpawnComponent;

        public EnemyCheatHandler(EnemySpawnComponent enemySpawnComponent)
        {
            _enemySpawnComponent = enemySpawnComponent ??
                throw new ArgumentNullException(nameof(enemySpawnComponent));
        }

        [CheatCommand("enemy_spawn", "Enemy")]
        private string SpawnEnemy()
        {
            if (!_enemySpawnComponent.SpawnEnemy())
            {
                throw new InvalidOperationException(
                    "Enemy spawn is only available in a running Solo or Host session.");
            }

            return "Enemy spawned.";
        }

        [CheatCommand("enemy_dash_spawn", "Enemy")]
        private string SpawnDashEnemy()
        {
            if (!_enemySpawnComponent.SpawnDashEnemy())
            {
                throw new InvalidOperationException(
                    "Dash enemy spawn is only available in a running Solo or Host session.");
            }

            return "Dash enemy spawned.";
        }

        [CheatCommand("enemy_despawn", "Enemy")]
        private string DespawnEnemies()
        {
            if (!_enemySpawnComponent.CanManageEnemies())
            {
                throw new InvalidOperationException(
                    "Enemy despawn is only available in a running Solo or Host session.");
            }

            int despawnedEnemiesCount = _enemySpawnComponent.DespawnEnemies();
            return $"Despawned enemies: {despawnedEnemiesCount}.";
        }
    }
}
