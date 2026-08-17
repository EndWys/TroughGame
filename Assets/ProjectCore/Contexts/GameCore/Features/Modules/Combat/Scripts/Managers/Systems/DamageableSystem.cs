using System;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public sealed class DamageableSystem : IDamageableSystem
    {
        private readonly IGameEntityComponentAccessor _gameEntityComponentAccessor;

        public DamageableSystem(IGameEntityComponentAccessor gameEntityComponentAccessor)
        {
            _gameEntityComponentAccessor = gameEntityComponentAccessor ??
                throw new ArgumentNullException(nameof(gameEntityComponentAccessor));
        }

        public void FillEntityIds(
            Predicate<BaseDamageableComponent> predicate,
            ICollection<NetworkEntityIdData> results)
        {
            _gameEntityComponentAccessor.FillEntityIds(predicate, results);
        }

        public bool TryFindEntityId(
            Predicate<BaseDamageableComponent> predicate,
            out NetworkEntityIdData entityId)
        {
            return _gameEntityComponentAccessor.TryFindEntityId(predicate, out entityId);
        }

        public void FillEntityIdsSortedBy<TSortKey>(
            Func<BaseDamageableComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityIdData> results)
        {
            _gameEntityComponentAccessor.FillEntityIdsSortedBy(sortKeySelector, results);
        }

        public bool TryFindFirstEntityIdSortedBy<TSortKey>(
            Func<BaseDamageableComponent, TSortKey> sortKeySelector,
            out NetworkEntityIdData entityId)
        {
            return _gameEntityComponentAccessor.TryFindFirstEntityIdSortedBy(sortKeySelector, out entityId);
        }

        public void ApplyToAll(Action<BaseDamageableComponent> action)
        {
            _gameEntityComponentAccessor.ApplyToAll(action);
        }
    }
}
