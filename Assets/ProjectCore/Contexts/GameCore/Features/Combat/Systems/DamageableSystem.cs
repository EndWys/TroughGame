using System;
using System.Collections.Generic;
using Domain;
using ProjectCore.GameCore;

namespace ProjectCore.GameCore
{
    public sealed class DamageableSystem : IDamageableSystem
    {
        private readonly IGameEntityComponentAccessor _gameEntityComponentAccessor;

        public DamageableSystem(NetworkEntityRegistry networkEntityRegistry)
        {
            _gameEntityComponentAccessor = new GameEntityComponentAccessor(networkEntityRegistry);
        }

        public void FillEntityIds(
            Predicate<BaseDamageableComponent> predicate,
            ICollection<NetworkEntityId> results)
        {
            _gameEntityComponentAccessor.FillEntityIds(predicate, results);
        }

        public bool TryFindEntityId(
            Predicate<BaseDamageableComponent> predicate,
            out NetworkEntityId entityId)
        {
            return _gameEntityComponentAccessor.TryFindEntityId(predicate, out entityId);
        }

        public void FillEntityIdsSortedBy<TSortKey>(
            Func<BaseDamageableComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityId> results)
        {
            _gameEntityComponentAccessor.FillEntityIdsSortedBy(sortKeySelector, results);
        }

        public bool TryFindFirstEntityIdSortedBy<TSortKey>(
            Func<BaseDamageableComponent, TSortKey> sortKeySelector,
            out NetworkEntityId entityId)
        {
            return _gameEntityComponentAccessor.TryFindFirstEntityIdSortedBy(sortKeySelector, out entityId);
        }

        public void ApplyToAll(Action<BaseDamageableComponent> action)
        {
            _gameEntityComponentAccessor.ApplyToAll(action);
        }
    }
}
