using System;
using System.Collections.Generic;
using Domain;

namespace GameCore.Combat
{
    public sealed class DamageSourceSystem : IDamageSourceSystem
    {
        private readonly IGameEntityComponentAccessor _gameEntityComponentAccessor;

        public DamageSourceSystem(NetworkEntityRegistry networkEntityRegistry)
        {
            _gameEntityComponentAccessor = new GameEntityComponentAccessor(networkEntityRegistry);
        }

        public void FillEntityIds(
            Predicate<BaseDamageSourceComponent> predicate,
            ICollection<NetworkEntityId> results)
        {
            _gameEntityComponentAccessor.FillEntityIds(predicate, results);
        }

        public bool TryFindEntityId(
            Predicate<BaseDamageSourceComponent> predicate,
            out NetworkEntityId entityId)
        {
            return _gameEntityComponentAccessor.TryFindEntityId(predicate, out entityId);
        }

        public void FillEntityIdsSortedBy<TSortKey>(
            Func<BaseDamageSourceComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityId> results)
        {
            _gameEntityComponentAccessor.FillEntityIdsSortedBy(sortKeySelector, results);
        }

        public bool TryFindFirstEntityIdSortedBy<TSortKey>(
            Func<BaseDamageSourceComponent, TSortKey> sortKeySelector,
            out NetworkEntityId entityId)
        {
            return _gameEntityComponentAccessor.TryFindFirstEntityIdSortedBy(sortKeySelector, out entityId);
        }

        public void ApplyToAll(Action<BaseDamageSourceComponent> action)
        {
            _gameEntityComponentAccessor.ApplyToAll(action);
        }
    }
}
