using System;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public interface IGameEntityComponentAccessor
    {
        void FillEntityIds<TComponent>(
            Predicate<TComponent> predicate,
            ICollection<NetworkEntityIdData> results)
            where TComponent : class;

        bool TryFindEntityId<TComponent>(
            Predicate<TComponent> predicate,
            out NetworkEntityIdData entityId)
            where TComponent : class;

        void FillEntityIdsSortedBy<TComponent, TSortKey>(
            Func<TComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityIdData> results)
            where TComponent : class;

        bool TryFindFirstEntityIdSortedBy<TComponent, TSortKey>(
            Func<TComponent, TSortKey> sortKeySelector,
            out NetworkEntityIdData entityId)
            where TComponent : class;

        void ApplyToAll<TComponent>(Action<TComponent> action)
            where TComponent : class;
    }
}
