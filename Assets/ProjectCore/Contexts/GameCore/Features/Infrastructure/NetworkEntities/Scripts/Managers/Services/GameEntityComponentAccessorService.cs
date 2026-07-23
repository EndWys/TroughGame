using System;
using System.Collections.Generic;
using Domain;
using ProjectCore.GameCore;

namespace ProjectCore.GameCore
{
    public sealed class GameEntityComponentAccessorService : IGameEntityComponentAccessor
    {
        private readonly NetworkEntityRegistry _networkEntityRegistry;

        public GameEntityComponentAccessorService(NetworkEntityRegistry networkEntityRegistry)
        {
            _networkEntityRegistry = networkEntityRegistry ?? throw new ArgumentNullException(nameof(networkEntityRegistry));
        }

        public void FillEntityIds<TComponent>(
            Predicate<TComponent> predicate,
            ICollection<NetworkEntityIdData> results)
            where TComponent : class
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

        public bool TryFindEntityId<TComponent>(
            Predicate<TComponent> predicate,
            out NetworkEntityIdData entityId)
            where TComponent : class
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            var entityIds = new List<NetworkEntityIdData>();
            _networkEntityRegistry.FillEntityIdsWithComponent(predicate, entityIds);

            if (entityIds.Count > 0)
            {
                entityId = entityIds[0];
                return true;
            }

            entityId = NetworkEntityIdData.None;
            return false;
        }

        public void FillEntityIdsSortedBy<TComponent, TSortKey>(
            Func<TComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityIdData> results)
            where TComponent : class
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
            var entityIds = new List<NetworkEntityIdData>();
            _networkEntityRegistry.FillEntityIdsWithComponent<TComponent>(entityIds);

            foreach (NetworkEntityIdData entityId in entityIds)
            {
                if (_networkEntityRegistry.TryGetComponent(entityId, out TComponent component))
                {
                    sortBuffer.Add(new EntitySortData<TSortKey>(
                        entityId,
                        sortKeySelector.Invoke(component)));
                }
            }

            sortBuffer.Sort((first, second) => Comparer<TSortKey>.Default.Compare(first.SortKey, second.SortKey));

            foreach (EntitySortData<TSortKey> result in sortBuffer)
            {
                results.Add(result.EntityId);
            }
        }

        public bool TryFindFirstEntityIdSortedBy<TComponent, TSortKey>(
            Func<TComponent, TSortKey> sortKeySelector,
            out NetworkEntityIdData entityId)
            where TComponent : class
        {
            if (sortKeySelector == null)
            {
                throw new ArgumentNullException(nameof(sortKeySelector));
            }

            var hasResult = false;
            var bestSortKey = default(TSortKey);
            entityId = NetworkEntityIdData.None;
            var entityIds = new List<NetworkEntityIdData>();
            _networkEntityRegistry.FillEntityIdsWithComponent<TComponent>(entityIds);

            foreach (NetworkEntityIdData candidateEntityId in entityIds)
            {
                if (!_networkEntityRegistry.TryGetComponent(candidateEntityId, out TComponent component))
                {
                    continue;
                }

                TSortKey sortKey = sortKeySelector.Invoke(component);

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

        public void ApplyToAll<TComponent>(Action<TComponent> action)
            where TComponent : class
        {
            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var components = new List<TComponent>();
            _networkEntityRegistry.FillComponents(components);

            foreach (TComponent component in components)
            {
                action.Invoke(component);
            }
        }

        private readonly struct EntitySortData<TSortKey>
        {
            public readonly NetworkEntityIdData EntityId;
            public readonly TSortKey SortKey;

            public EntitySortData(NetworkEntityIdData entityId, TSortKey sortKey)
            {
                EntityId = entityId;
                SortKey = sortKey;
            }
        }
    }
}
