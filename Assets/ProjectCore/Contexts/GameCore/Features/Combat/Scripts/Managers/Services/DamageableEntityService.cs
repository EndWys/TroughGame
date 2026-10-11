using System;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public sealed class DamageableEntityService : IDamageableEntityService
    {
        private readonly IGameEntityComponentService _gameEntityComponentService;

        public DamageableEntityService(IGameEntityComponentService gameEntityComponentService)
        {
            _gameEntityComponentService = gameEntityComponentService ??
                throw new ArgumentNullException(nameof(gameEntityComponentService));
        }

        public void FillEntityIds(
            Predicate<BaseDamageableComponent> predicate,
            ICollection<NetworkEntityIdData> results)
        {
            _gameEntityComponentService.FillEntityIds(predicate, results);
        }

        public bool TryFindEntityId(
            Predicate<BaseDamageableComponent> predicate,
            out NetworkEntityIdData entityId)
        {
            return _gameEntityComponentService.TryFindEntityId(predicate, out entityId);
        }

        public void FillEntityIdsSortedBy<TSortKey>(
            Func<BaseDamageableComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityIdData> results)
        {
            _gameEntityComponentService.FillEntityIdsSortedBy(sortKeySelector, results);
        }

        public bool TryFindFirstEntityIdSortedBy<TSortKey>(
            Func<BaseDamageableComponent, TSortKey> sortKeySelector,
            out NetworkEntityIdData entityId)
        {
            return _gameEntityComponentService.TryFindFirstEntityIdSortedBy(sortKeySelector, out entityId);
        }

        public void ApplyToAll(Action<BaseDamageableComponent> action)
        {
            _gameEntityComponentService.ApplyToAll(action);
        }
    }
}
