using System;
using System.Collections.Generic;
using Domain;
using ProjectCore.GameCore;

namespace ProjectCore.GameCore
{
    public sealed class DamageSourceSystem : IDamageSourceSystem
    {
        private readonly IGameEntityComponentAccessor _gameEntityComponentAccessor;

        public DamageSourceSystem(IGameEntityComponentAccessor gameEntityComponentAccessor)
        {
            _gameEntityComponentAccessor = gameEntityComponentAccessor ??
                throw new ArgumentNullException(nameof(gameEntityComponentAccessor));
        }

        public void FillEntityIds(
            Predicate<BaseDamageSourceComponent> predicate,
            ICollection<NetworkEntityIdData> results)
        {
            _gameEntityComponentAccessor.FillEntityIds(predicate, results);
        }

        public bool TryFindEntityId(
            Predicate<BaseDamageSourceComponent> predicate,
            out NetworkEntityIdData entityId)
        {
            return _gameEntityComponentAccessor.TryFindEntityId(predicate, out entityId);
        }

        public void FillEntityIdsSortedBy<TSortKey>(
            Func<BaseDamageSourceComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityIdData> results)
        {
            _gameEntityComponentAccessor.FillEntityIdsSortedBy(sortKeySelector, results);
        }

        public bool TryFindFirstEntityIdSortedBy<TSortKey>(
            Func<BaseDamageSourceComponent, TSortKey> sortKeySelector,
            out NetworkEntityIdData entityId)
        {
            return _gameEntityComponentAccessor.TryFindFirstEntityIdSortedBy(sortKeySelector, out entityId);
        }

        public void ApplyToAll(Action<BaseDamageSourceComponent> action)
        {
            _gameEntityComponentAccessor.ApplyToAll(action);
        }
    }
}
