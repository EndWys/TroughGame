using System;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public interface IMovementSystem
    {
        void FillEntityIds<TMovementComponent>(
            Predicate<TMovementComponent> predicate,
            ICollection<NetworkEntityIdData> results)
            where TMovementComponent : class;

        bool TryFindEntityId<TMovementComponent>(
            Predicate<TMovementComponent> predicate,
            out NetworkEntityIdData entityId)
            where TMovementComponent : class;

        void FillEntityIdsSortedBy<TMovementComponent, TSortKey>(
            Func<TMovementComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityIdData> results)
            where TMovementComponent : class;

        bool TryFindFirstEntityIdSortedBy<TMovementComponent, TSortKey>(
            Func<TMovementComponent, TSortKey> sortKeySelector,
            out NetworkEntityIdData entityId)
            where TMovementComponent : class;

        void ApplyToAll<TMovementComponent>(
            Action<TMovementComponent> action)
            where TMovementComponent : class;
    }
}
