using System;
using System.Collections.Generic;
using Domain;
using ProjectCore.GameCore;

namespace ProjectCore.GameCore
{
    public interface IDamageSourceSystem
    {
        void FillEntityIds(
            Predicate<BaseDamageSourceComponent> predicate,
            ICollection<NetworkEntityIdData> results);

        bool TryFindEntityId(
            Predicate<BaseDamageSourceComponent> predicate,
            out NetworkEntityIdData entityId);

        void FillEntityIdsSortedBy<TSortKey>(
            Func<BaseDamageSourceComponent, TSortKey> sortKeySelector,
            ICollection<NetworkEntityIdData> results);

        bool TryFindFirstEntityIdSortedBy<TSortKey>(
            Func<BaseDamageSourceComponent, TSortKey> sortKeySelector,
            out NetworkEntityIdData entityId);

        void ApplyToAll(Action<BaseDamageSourceComponent> action);
    }
}
