using System;
using System.Collections.Generic;
using Domain;
using ProjectCore.GameCore;

namespace ProjectCore.GameCore
{
    public sealed class MovementSystem : IMovementSystem
    {
        private readonly IGameEntityComponentAccessor _gameEntityComponentAccessor;

        public MovementSystem(NetworkEntityRegistry networkEntityRegistry)
        {
            _gameEntityComponentAccessor = new GameEntityComponentAccessor(networkEntityRegistry);
        }

        public void FillEntityIds<TMovementComponent>(
            Predicate<TMovementComponent> predicate,
            ICollection<NetworkEntityId> results)
            where TMovementComponent : class
        {
            _gameEntityComponentAccessor.FillEntityIds(predicate, results);
        }

        public bool TryFindEntityId<TMovementComponent>(
            Predicate<TMovementComponent> predicate,
            out NetworkEntityId entityId)
            where TMovementComponent : class
        {
            return _gameEntityComponentAccessor.TryFindEntityId(predicate, out entityId);
        }

        public void FillEntityIdsSortedBy<TMovementComponent, TSortKey>(
            Func<TMovementComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityId> results)
            where TMovementComponent : class
        {
            _gameEntityComponentAccessor.FillEntityIdsSortedBy(sortKeySelector, results);
        }

        public bool TryFindFirstEntityIdSortedBy<TMovementComponent, TSortKey>(
            Func<TMovementComponent, TSortKey> sortKeySelector,
            out NetworkEntityId entityId)
            where TMovementComponent : class
        {
            return _gameEntityComponentAccessor.TryFindFirstEntityIdSortedBy(sortKeySelector, out entityId);
        }

        public void ApplyToAll<TMovementComponent>(
            Action<TMovementComponent> action)
            where TMovementComponent : class
        {
            _gameEntityComponentAccessor.ApplyToAll(action);
        }
    }
}
