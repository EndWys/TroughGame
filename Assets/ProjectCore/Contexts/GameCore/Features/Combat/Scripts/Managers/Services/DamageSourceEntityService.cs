using System;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public sealed class DamageSourceEntityService : IDamageSourceEntityService
    {
        private readonly IGameEntityComponentService _gameEntityComponentService;

        public DamageSourceEntityService(IGameEntityComponentService gameEntityComponentService)
        {
            _gameEntityComponentService = gameEntityComponentService ??
                throw new ArgumentNullException(nameof(gameEntityComponentService));
        }

        public void FillEntityIds(
            Predicate<BaseDamageSourceComponent> predicate,
            ICollection<NetworkEntityIdData> results)
        {
            _gameEntityComponentService.FillEntityIds(predicate, results);
        }

        public bool TryFindEntityId(
            Predicate<BaseDamageSourceComponent> predicate,
            out NetworkEntityIdData entityId)
        {
            return _gameEntityComponentService.TryFindEntityId(predicate, out entityId);
        }

        public void FillEntityIdsSortedBy<TSortKey>(
            Func<BaseDamageSourceComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityIdData> results)
        {
            _gameEntityComponentService.FillEntityIdsSortedBy(sortKeySelector, results);
        }

        public bool TryFindFirstEntityIdSortedBy<TSortKey>(
            Func<BaseDamageSourceComponent, TSortKey> sortKeySelector,
            out NetworkEntityIdData entityId)
        {
            return _gameEntityComponentService.TryFindFirstEntityIdSortedBy(sortKeySelector, out entityId);
        }

        public void ApplyToAll(Action<BaseDamageSourceComponent> action)
        {
            _gameEntityComponentService.ApplyToAll(action);
        }
    }
}
