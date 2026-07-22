using System;
using System.Collections.Generic;
using Domain;

namespace GameCore
{
    public interface IGameEntityComponentAccessor
    {
        void FillEntityIds<TComponent>(
            Predicate<TComponent> predicate,
            ICollection<NetworkEntityId> results)
            where TComponent : class;

        bool TryFindEntityId<TComponent>(
            Predicate<TComponent> predicate,
            out NetworkEntityId entityId)
            where TComponent : class;

        void FillEntityIdsSortedBy<TComponent, TSortKey>(
            Func<TComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityId> results)
            where TComponent : class;

        bool TryFindFirstEntityIdSortedBy<TComponent, TSortKey>(
            Func<TComponent, TSortKey> sortKeySelector,
            out NetworkEntityId entityId)
            where TComponent : class;

        void ApplyToAll<TComponent>(Action<TComponent> action)
            where TComponent : class;
    }
}
