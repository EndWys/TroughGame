using System;
using System.Collections.Generic;
using Domain;

namespace GameCore.Movement
{
    public sealed class MovementSystem : IMovementSystem
    {
        private readonly NetworkEntityRegistry _networkEntityRegistry;

        public MovementSystem(NetworkEntityRegistry networkEntityRegistry)
        {
            _networkEntityRegistry = networkEntityRegistry ?? throw new ArgumentNullException(nameof(networkEntityRegistry));
        }

        public void FillEntityIds<TMovementComponent>(
            Predicate<TMovementComponent> predicate,
            ICollection<NetworkEntityId> results)
            where TMovementComponent : class
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            _networkEntityRegistry.FillEntityIdsWithComponent(predicate, results);
        }

        public bool TryFindEntityId<TMovementComponent>(
            Predicate<TMovementComponent> predicate,
            out NetworkEntityId entityId)
            where TMovementComponent : class
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var entityIds = new List<NetworkEntityId>();
            _networkEntityRegistry.FillEntityIdsWithComponent(predicate, entityIds);

            if (entityIds.Count > 0)
            {
                entityId = entityIds[0];
                return true;
            }

            entityId = NetworkEntityId.None;
            return false;
        }

        public void FillEntityIdsSortedBy<TMovementComponent, TSortKey>(
            Func<TMovementComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityId> results)
            where TMovementComponent : class
        {
            if (sortKeySelector == null)
            {
                throw new ArgumentNullException(nameof(sortKeySelector));
            }

            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            var sortBuffer = new List<EntitySortData<TSortKey>>();
            var entityIds = new List<NetworkEntityId>();
            _networkEntityRegistry.FillEntityIdsWithComponent<TMovementComponent>(entityIds);

            foreach (NetworkEntityId entityId in entityIds)
            {
                if (_networkEntityRegistry.TryGetComponent(entityId, out TMovementComponent movementComponent))
                {
                    sortBuffer.Add(new EntitySortData<TSortKey>(
                        entityId,
                        sortKeySelector.Invoke(movementComponent)));
                }
            }

            sortBuffer.Sort((first, second) => Comparer<TSortKey>.Default.Compare(first.SortKey, second.SortKey));

            foreach (EntitySortData<TSortKey> result in sortBuffer)
            {
                results.Add(result.EntityId);
            }
        }

        public bool TryFindFirstEntityIdSortedBy<TMovementComponent, TSortKey>(
            Func<TMovementComponent, TSortKey> sortKeySelector,
            out NetworkEntityId entityId)
            where TMovementComponent : class
        {
            if (sortKeySelector == null)
            {
                throw new ArgumentNullException(nameof(sortKeySelector));
            }

            var hasResult = false;
            var bestSortKey = default(TSortKey);
            entityId = NetworkEntityId.None;
            var entityIds = new List<NetworkEntityId>();
            _networkEntityRegistry.FillEntityIdsWithComponent<TMovementComponent>(entityIds);

            foreach (NetworkEntityId candidateEntityId in entityIds)
            {
                if (!_networkEntityRegistry.TryGetComponent(candidateEntityId, out TMovementComponent movementComponent))
                {
                    continue;
                }

                TSortKey sortKey = sortKeySelector.Invoke(movementComponent);

                if (hasResult && Comparer<TSortKey>.Default.Compare(sortKey, bestSortKey) >= 0)
                {
                    continue;
                }

                hasResult = true;
                bestSortKey = sortKey;
                entityId = candidateEntityId;
            }

            return hasResult;
        }

        public void ApplyToAll<TMovementComponent>(
            Action<TMovementComponent> action)
            where TMovementComponent : class
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var components = new List<TMovementComponent>();
            _networkEntityRegistry.FillComponents(components);

            foreach (TMovementComponent movementComponent in components)
            {
                action.Invoke(movementComponent);
            }
        }

        private readonly struct EntitySortData<TSortKey>
        {
            public readonly NetworkEntityId EntityId;
            public readonly TSortKey SortKey;

            public EntitySortData(NetworkEntityId entityId, TSortKey sortKey)
            {
                EntityId = entityId;
                SortKey = sortKey;
            }
        }
    }
}
