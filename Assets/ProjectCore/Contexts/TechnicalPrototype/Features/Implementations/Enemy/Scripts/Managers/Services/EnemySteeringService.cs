using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemySteeringService : IEnemySteeringService
    {
        private readonly INetworkBehaviourAccessor _networkBehaviourAccessor;
        private readonly NetworkEntityRegistry _networkEntityRegistry;

        public EnemySteeringService(
            INetworkBehaviourAccessor networkBehaviourAccessor,
            NetworkEntityRegistry networkEntityRegistry)
        {
            _networkBehaviourAccessor = networkBehaviourAccessor ??
                throw new ArgumentNullException(nameof(networkBehaviourAccessor));
            _networkEntityRegistry = networkEntityRegistry ??
                throw new ArgumentNullException(nameof(networkEntityRegistry));
        }

        public Vector2 ApproachPosition { get; private set; }

        public Vector2 CalculateDirection(
            NetworkEntityIdData targetEntityId,
            Vector2 targetPosition,
            float stoppingDistance,
            float separationDistance,
            float separationWeight)
        {
            EnemyNetworkEntityComponent enemy = _networkBehaviourAccessor.ParentNetworkBehaviour as
                EnemyNetworkEntityComponent;

            if (enemy == null)
            {
                throw new InvalidOperationException("Enemy steering requires an enemy network entity.");
            }

            Vector2 position = enemy.transform.position;
            IReadOnlyList<BaseNetworkEntityRoot> enemies =
                _networkEntityRegistry.GetByType(EnemyNetworkEntityConstants.Enemy);
            int approachingEnemiesCount = GetApproachingEnemiesCount(enemies, targetEntityId);
            int approachIndex = GetApproachIndex(enemies, enemy, targetEntityId);
            ApproachPosition = targetPosition + GetApproachOffset(
                approachIndex,
                approachingEnemiesCount,
                stoppingDistance);
            Vector2 direction = ApproachPosition - position;
            Vector2 separation = GetSeparation(enemies, enemy, targetEntityId, position, separationDistance);

            return Vector2.ClampMagnitude(
                direction.normalized + separation * Mathf.Max(0f, separationWeight),
                1f);
        }

        private static int GetApproachingEnemiesCount(
            IReadOnlyList<BaseNetworkEntityRoot> enemies,
            NetworkEntityIdData targetEntityId)
        {
            int count = 0;

            foreach (BaseNetworkEntityRoot enemy in enemies)
            {
                if (Targets(enemy, targetEntityId))
                {
                    count++;
                }
            }

            return Mathf.Max(1, count);
        }

        private static int GetApproachIndex(
            IReadOnlyList<BaseNetworkEntityRoot> enemies,
            EnemyNetworkEntityComponent self,
            NetworkEntityIdData targetEntityId)
        {
            int index = 0;

            foreach (BaseNetworkEntityRoot enemy in enemies)
            {
                if (!Targets(enemy, targetEntityId))
                {
                    continue;
                }

                if (enemy == self)
                {
                    return index;
                }

                index++;
            }

            return 0;
        }

        private static Vector2 GetApproachOffset(int index, int count, float stoppingDistance)
        {
            float angle = index / (float)count * Mathf.PI * 2f;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Mathf.Max(0f, stoppingDistance);
        }

        private static Vector2 GetSeparation(
            IReadOnlyList<BaseNetworkEntityRoot> enemies,
            EnemyNetworkEntityComponent self,
            NetworkEntityIdData targetEntityId,
            Vector2 position,
            float separationDistance)
        {
            if (separationDistance <= 0f)
            {
                return Vector2.zero;
            }

            Vector2 separation = Vector2.zero;

            foreach (BaseNetworkEntityRoot enemy in enemies)
            {
                if (enemy == self || !Targets(enemy, targetEntityId))
                {
                    continue;
                }

                Vector2 offset = position - (Vector2)enemy.transform.position;
                float distance = offset.magnitude;

                if (distance <= Mathf.Epsilon || distance >= separationDistance)
                {
                    continue;
                }

                separation += offset / distance * (1f - distance / separationDistance);
            }

            return separation;
        }

        private static bool Targets(BaseNetworkEntityRoot enemy, NetworkEntityIdData targetEntityId)
        {
            return enemy != null &&
                   enemy.TryGetRootContract(out IEnemyTargetAccessor targetAccessor) &&
                   targetAccessor.TargetEntityId == targetEntityId;
        }
    }
}
