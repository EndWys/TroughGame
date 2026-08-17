using System;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public interface IDamageableSystem
    {
        void FillEntityIds(
            Predicate<BaseDamageableComponent> predicate,
            ICollection<NetworkEntityIdData> results);

        bool TryFindEntityId(
            Predicate<BaseDamageableComponent> predicate,
            out NetworkEntityIdData entityId);

        void FillEntityIdsSortedBy<TSortKey>(
            Func<BaseDamageableComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityIdData> results);

        bool TryFindFirstEntityIdSortedBy<TSortKey>(
            Func<BaseDamageableComponent, TSortKey> sortKeySelector,
            out NetworkEntityIdData entityId);

        void ApplyToAll(Action<BaseDamageableComponent> action);
    }
}
