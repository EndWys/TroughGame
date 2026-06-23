using System;
using System.Collections.Generic;
using Zenject;

namespace Domain
{
    public sealed class NetworkEntitySpawner
    {
        private readonly Dictionary<Type, INetworkEntityFactory> _factoriesByPayloadType = new();

        [Inject]
        public NetworkEntitySpawner(List<INetworkEntityFactory> factories)
        {
            if (factories == null)
            {
                throw new ArgumentNullException(nameof(factories));
            }

            foreach (INetworkEntityFactory factory in factories)
            {
                RegisterFactory(factory);
            }
        }

        public BaseNetworkEntityRoot Spawn<TPayload>(TPayload payload)
            where TPayload : INetworkEntitySpawnPayload
        {
            if (!_factoriesByPayloadType.TryGetValue(typeof(TPayload), out INetworkEntityFactory factory))
            {
                throw new InvalidOperationException($"No network entity factory registered for payload '{typeof(TPayload).Name}'.");
            }

            return factory.Spawn(payload);
        }

        public void Despawn(BaseNetworkEntityRoot entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }

            if (!entity.EntityId.IsValid)
            {
                throw new ArgumentException("Cannot despawn network entity without valid id.", nameof(entity));
            }

            foreach (INetworkEntityFactory factory in _factoriesByPayloadType.Values)
            {
                if (factory.EntityType == entity.EntityId.Type)
                {
                    factory.Despawn(entity);
                    return;
                }
            }

            throw new InvalidOperationException($"No network entity factory registered for entity type '{entity.EntityId.Type}'.");
        }

        private void RegisterFactory(INetworkEntityFactory factory)
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            if (_factoriesByPayloadType.ContainsKey(factory.PayloadType))
            {
                throw new InvalidOperationException($"Network entity factory for payload '{factory.PayloadType.Name}' is already registered.");
            }

            _factoriesByPayloadType.Add(factory.PayloadType, factory);
        }
    }
}
