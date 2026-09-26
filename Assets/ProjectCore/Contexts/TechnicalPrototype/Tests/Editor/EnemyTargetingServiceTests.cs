using System;
using System.Collections.Generic;
using NUnit.Framework;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyTargetingServiceTests
    {
        [Test]
        public void TryFindNearestTargetReturnsClosestAvailableTarget()
        {
            NetworkEntityIdData nearestId = new(new NetworkEntityTypeData("player"), 2);
            var accessor = new TestGameEntityComponentAccessor(new Dictionary<
                NetworkEntityIdData,
                ITargetableNetworkEntity>
            {
                { new NetworkEntityIdData(new NetworkEntityTypeData("player"), 0), new TestTarget(new(8f, 0f)) },
                { nearestId, new TestTarget(new(2f, 0f)) },
                { new NetworkEntityIdData(new NetworkEntityTypeData("player"), 1), new TestTarget(new(4f, 0f)) },
            });
            var service = new EnemyTargetingService(accessor, new NetworkEntityRegistry());

            bool found = service.TryFindNearestTarget(Vector2.zero, out NetworkEntityIdData result);

            Assert.That(found, Is.True);
            Assert.That(result, Is.EqualTo(nearestId));
        }

        [Test]
        public void HasAvailableTargetReturnsFalseWhenNoTargetIsRegistered()
        {
            var service = new EnemyTargetingService(
                new TestGameEntityComponentAccessor(new Dictionary<
                    NetworkEntityIdData,
                    ITargetableNetworkEntity>()),
                new NetworkEntityRegistry());

            Assert.That(service.HasAvailableTarget(), Is.False);
        }

        private sealed class TestTarget : ITargetableNetworkEntity
        {
            public TestTarget(Vector2 targetPosition)
            {
                TargetPosition = targetPosition;
            }

            public Vector2 TargetPosition { get; }
        }

        private sealed class TestGameEntityComponentAccessor : IGameEntityComponentAccessor
        {
            private readonly IReadOnlyDictionary<NetworkEntityIdData, ITargetableNetworkEntity> _targets;

            public TestGameEntityComponentAccessor(
                IReadOnlyDictionary<NetworkEntityIdData, ITargetableNetworkEntity> targets)
            {
                _targets = targets;
            }

            public void FillEntityIds<TComponent>(
                Predicate<TComponent> predicate,
                ICollection<NetworkEntityIdData> results)
                where TComponent : class
            {
                foreach (KeyValuePair<NetworkEntityIdData, ITargetableNetworkEntity> target in _targets)
                {
                    if (target.Value is TComponent component && predicate(component))
                    {
                        results.Add(target.Key);
                    }
                }
            }

            public bool TryFindEntityId<TComponent>(
                Predicate<TComponent> predicate,
                out NetworkEntityIdData entityId)
                where TComponent : class
            {
                foreach (KeyValuePair<NetworkEntityIdData, ITargetableNetworkEntity> target in _targets)
                {
                    if (target.Value is TComponent component && predicate(component))
                    {
                        entityId = target.Key;
                        return true;
                    }
                }

                entityId = NetworkEntityIdData.None;
                return false;
            }

            public void FillEntityIdsSortedBy<TComponent, TSortKey>(
                Func<TComponent, TSortKey> sortKeySelector,
                ICollection<NetworkEntityIdData> results)
                where TComponent : class
            {
                foreach (NetworkEntityIdData entityId in GetSortedEntityIds(sortKeySelector))
                {
                    results.Add(entityId);
                }
            }

            public bool TryFindFirstEntityIdSortedBy<TComponent, TSortKey>(
                Func<TComponent, TSortKey> sortKeySelector,
                out NetworkEntityIdData entityId)
                where TComponent : class
            {
                foreach (NetworkEntityIdData candidateId in GetSortedEntityIds(sortKeySelector))
                {
                    entityId = candidateId;
                    return true;
                }

                entityId = NetworkEntityIdData.None;
                return false;
            }

            public void ApplyToAll<TComponent>(Action<TComponent> action)
                where TComponent : class
            {
                foreach (ITargetableNetworkEntity target in _targets.Values)
                {
                    if (target is TComponent component)
                    {
                        action(component);
                    }
                }
            }

            private IEnumerable<NetworkEntityIdData> GetSortedEntityIds<TComponent, TSortKey>(
                Func<TComponent, TSortKey> sortKeySelector)
                where TComponent : class
            {
                var values = new List<KeyValuePair<NetworkEntityIdData, TSortKey>>();

                foreach (KeyValuePair<NetworkEntityIdData, ITargetableNetworkEntity> target in _targets)
                {
                    if (target.Value is TComponent component)
                    {
                        values.Add(new(target.Key, sortKeySelector(component)));
                    }
                }

                values.Sort((first, second) => Comparer<TSortKey>.Default.Compare(first.Value, second.Value));

                foreach (KeyValuePair<NetworkEntityIdData, TSortKey> value in values)
                {
                    yield return value.Key;
                }
            }
        }
    }
}
