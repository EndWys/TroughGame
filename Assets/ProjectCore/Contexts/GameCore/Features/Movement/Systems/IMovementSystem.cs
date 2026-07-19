using System;
using System.Collections.Generic;
using Domain;

namespace GameCore.Movement
{
    public interface IMovementSystem
    {
        void FillEntityIds<TMovementComponent>(
            Predicate<TMovementComponent> predicate,
            ICollection<NetworkEntityId> results)
            where TMovementComponent : class;

        bool TryFindEntityId<TMovementComponent>(
            Predicate<TMovementComponent> predicate,
            out NetworkEntityId entityId)
            where TMovementComponent : class;

        void FillEntityIdsSortedBy<TMovementComponent, TSortKey>(
            Func<TMovementComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityId> results)
            where TMovementComponent : class;

        bool TryFindFirstEntityIdSortedBy<TMovementComponent, TSortKey>(
            Func<TMovementComponent, TSortKey> sortKeySelector,
            out NetworkEntityId entityId)
            where TMovementComponent : class;

        void ApplyToAll<TMovementComponent>(
            Action<TMovementComponent> action)
            where TMovementComponent : class;
    }
}
