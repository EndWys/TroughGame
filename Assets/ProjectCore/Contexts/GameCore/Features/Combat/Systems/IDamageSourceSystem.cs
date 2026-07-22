using System;
using System.Collections.Generic;
using Domain;

namespace GameCore.Combat
{
    public interface IDamageSourceSystem
    {
        void FillEntityIds(
            Predicate<BaseDamageSourceComponent> predicate,
            ICollection<NetworkEntityId> results);

        bool TryFindEntityId(
            Predicate<BaseDamageSourceComponent> predicate,
            out NetworkEntityId entityId);

        void FillEntityIdsSortedBy<TSortKey>(
            Func<BaseDamageSourceComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityId> results);

        bool TryFindFirstEntityIdSortedBy<TSortKey>(
            Func<BaseDamageSourceComponent, TSortKey> sortKeySelector,
            out NetworkEntityId entityId);

        void ApplyToAll(Action<BaseDamageSourceComponent> action);
    }
}
