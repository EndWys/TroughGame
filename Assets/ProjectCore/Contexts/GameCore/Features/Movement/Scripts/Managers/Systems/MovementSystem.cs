using System;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public sealed class MovementSystem : IMovementSystem
    {
        private readonly IGameEntityComponentService _gameEntityComponentService;

        public MovementSystem(IGameEntityComponentService gameEntityComponentService)
        {
            _gameEntityComponentService = gameEntityComponentService ??
                throw new ArgumentNullException(nameof(gameEntityComponentService));
        }

        public void FillEntityIds<TMovementComponent>(
            Predicate<TMovementComponent> predicate,
            ICollection<NetworkEntityIdData> results)
            where TMovementComponent : class
        {
            _gameEntityComponentService.FillEntityIds(predicate, results);
        }

        public bool TryFindEntityId<TMovementComponent>(
            Predicate<TMovementComponent> predicate,
            out NetworkEntityIdData entityId)
            where TMovementComponent : class
        {
            return _gameEntityComponentService.TryFindEntityId(predicate, out entityId);
        }

        public void FillEntityIdsSortedBy<TMovementComponent, TSortKey>(
            Func<TMovementComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityIdData> results)
            where TMovementComponent : class
        {
            _gameEntityComponentService.FillEntityIdsSortedBy(sortKeySelector, results);
        }

        public bool TryFindFirstEntityIdSortedBy<TMovementComponent, TSortKey>(
            Func<TMovementComponent, TSortKey> sortKeySelector,
            out NetworkEntityIdData entityId)
            where TMovementComponent : class
        {
            return _gameEntityComponentService.TryFindFirstEntityIdSortedBy(sortKeySelector, out entityId);
        }

        public void ApplyToAll<TMovementComponent>(
            Action<TMovementComponent> action)
            where TMovementComponent : class
        {
            _gameEntityComponentService.ApplyToAll(action);
        }
    }
}
