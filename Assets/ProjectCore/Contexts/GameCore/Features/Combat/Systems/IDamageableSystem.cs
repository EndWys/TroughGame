using System;
using System.Collections.Generic;
using Domain;
using ProjectCore.GameCore;

namespace ProjectCore.GameCore
{
    public interface IDamageableSystem
    {
        void FillEntityIds(
            Predicate<BaseDamageableComponent> predicate,
            ICollection<NetworkEntityId> results);

        bool TryFindEntityId(
            Predicate<BaseDamageableComponent> predicate,
            out NetworkEntityId entityId);

        void FillEntityIdsSortedBy<TSortKey>(
            Func<BaseDamageableComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityId> results);

        bool TryFindFirstEntityIdSortedBy<TSortKey>(
            Func<BaseDamageableComponent, TSortKey> sortKeySelector,
            out NetworkEntityId entityId);

        void ApplyToAll(Action<BaseDamageableComponent> action);
    }
}
