using System;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public sealed class NetworkEntityRegistry
    {
        private readonly Dictionary<NetworkEntityIdData, BaseNetworkEntityRoot> _entitiesById = new();

        private readonly Dictionary<NetworkEntityTypeData, List<BaseNetworkEntityRoot>> _entitiesByType = new();

        public IReadOnlyDictionary<NetworkEntityIdData, BaseNetworkEntityRoot> EntitiesById => _entitiesById;

        public void Register(BaseNetworkEntityRoot entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            NetworkEntityIdData entityId = entity.EntityId;

            if (!entityId.IsValid)
            {
                throw new ArgumentException("Cannot register network entity without valid id.", nameof(entity));
            }

            if (_entitiesById.TryGetValue(entityId, out BaseNetworkEntityRoot registeredEntity) &&
                registeredEntity != entity)
            {
                throw new InvalidOperationException($"Network entity id '{entityId}' is already registered.");
            }

            _entitiesById[entityId] = entity;

            if (!_entitiesByType.TryGetValue(entityId.Type, out List<BaseNetworkEntityRoot> typedEntities))
            {
                typedEntities = new List<BaseNetworkEntityRoot>();
                _entitiesByType.Add(entityId.Type, typedEntities);
            }

            if (!typedEntities.Contains(entity))
            {
                typedEntities.Add(entity);
            }
        }

        public void Unregister(BaseNetworkEntityRoot entity)
        {
            if (entity == null || !entity.EntityId.IsValid)
            {
                return;
            }

            NetworkEntityIdData entityId = entity.EntityId;

            if (_entitiesById.TryGetValue(entityId, out BaseNetworkEntityRoot registeredEntity) &&
                registeredEntity == entity)
            {
                _entitiesById.Remove(entityId);
            }

            if (!_entitiesByType.TryGetValue(entityId.Type, out List<BaseNetworkEntityRoot> typedEntities))
            {
                return;
            }

            typedEntities.Remove(entity);

            if (typedEntities.Count == 0)
            {
                _entitiesByType.Remove(entityId.Type);
            }
        }

        public bool TryGet(NetworkEntityIdData entityId, out BaseNetworkEntityRoot entity)
        {
            return _entitiesById.TryGetValue(entityId, out entity);
        }

        public bool TryGetComponent<TComponent>(NetworkEntityIdData entityId, out TComponent component)
            where TComponent : class
        {
            if (TryGet(entityId, out BaseNetworkEntityRoot entity))
            {
                return entity.TryGetEntityComponent(out component);
            }

            component = null;
            return false;
        }

        public IReadOnlyList<BaseNetworkEntityRoot> GetByType(NetworkEntityTypeData entityType)
        {
            return _entitiesByType.TryGetValue(entityType, out List<BaseNetworkEntityRoot> entities)
                ? entities
                : Array.Empty<BaseNetworkEntityRoot>();
        }

        public List<TComponent> GetComponentsByType<TComponent>(NetworkEntityTypeData entityType)
            where TComponent : class
        {
            var results = new List<TComponent>();
            FillComponentsByType(entityType, results);

            return results;
        }

        public void FillComponents<TComponent>(
            ICollection<TComponent> results)
            where TComponent : class
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            foreach (BaseNetworkEntityRoot entity in _entitiesById.Values)
            {
                entity.FillEntityComponents(results);
            }
        }

        public void FillComponentsByType<TComponent>(
            NetworkEntityTypeData entityType,
            ICollection<TComponent> results)
            where TComponent : class
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            if (!_entitiesByType.TryGetValue(entityType, out List<BaseNetworkEntityRoot> entities))
            {
                return;
            }

            foreach (BaseNetworkEntityRoot entity in entities)
            {
                entity.FillEntityComponents(results);
            }
        }

        public void FillEntityIdsWithComponent<TComponent>(
            ICollection<NetworkEntityIdData> results)
            where TComponent : class
        {
            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            foreach (KeyValuePair<NetworkEntityIdData, BaseNetworkEntityRoot> registeredEntity in _entitiesById)
            {
                if (registeredEntity.Value.TryGetEntityComponent<TComponent>(out _))
                {
                    results.Add(registeredEntity.Key);
                }
            }
        }

        public void FillEntityIdsWithComponent<TComponent>(
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

            var componentBuffer = new List<TComponent>();

            foreach (KeyValuePair<NetworkEntityIdData, BaseNetworkEntityRoot> registeredEntity in _entitiesById)
            {
                componentBuffer.Clear();
                registeredEntity.Value.FillEntityComponents(componentBuffer);

                if (componentBuffer.Exists(predicate))
                {
                    results.Add(registeredEntity.Key);
                }
            }
        }
    }
}
