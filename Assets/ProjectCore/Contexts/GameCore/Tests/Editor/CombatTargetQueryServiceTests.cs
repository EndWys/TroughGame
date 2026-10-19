using System;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class CombatTargetQueryServiceTests
    {
        [Test]
        public void QueryAppendsAvailableOverlappingTargetsAndExcludesSource()
        {
            var sourceId = new NetworkEntityIdData(NetworkEntityType.Player, 0);
            var targetId = new NetworkEntityIdData(NetworkEntityType.Enemy, 1);
            var distantId = new NetworkEntityIdData(NetworkEntityType.Enemy, 2);
            var unavailableId = new NetworkEntityIdData(NetworkEntityType.Enemy, 3);
            var service = new TestGameEntityComponentService();
            service.Add(sourceId, new TestHurtbox(true, Vector2.zero));
            service.Add(targetId, new TestHurtbox(true, Vector2.right));
            service.Add(distantId, new TestHurtbox(true, new Vector2(20f, 0f)));
            service.Add(unavailableId, new TestHurtbox(false, Vector2.zero));
            var existingId = new NetworkEntityIdData(NetworkEntityType.Enemy, 4);
            var results = new List<NetworkEntityIdData> { existingId };

            new CombatTargetQueryService(service).FillTargetIds(sourceId, Circle(Vector2.zero), results);

            Assert.That(results, Is.EqualTo(new[] { existingId, targetId }));
        }

        [Test]
        public void MultipleHurtboxesAndRepeatedQueriesDoNotAppendDuplicateEntities()
        {
            var targetId = new NetworkEntityIdData(NetworkEntityType.Enemy, 1);
            var otherId = new NetworkEntityIdData(NetworkEntityType.Enemy, 2);
            var service = new TestGameEntityComponentService();
            service.Add(targetId, new TestHurtbox(false, Vector2.zero));
            service.Add(targetId, new TestHurtbox(true, new Vector2(10f, 0f)));
            service.Add(targetId, new TestHurtbox(true, Vector2.zero));
            service.Add(targetId, new TestHurtbox(true, Vector2.right));
            service.Add(otherId, new TestHurtbox(true, Vector2.left));
            var query = new CombatTargetQueryService(service);
            var results = new List<NetworkEntityIdData>();

            query.FillTargetIds(NetworkEntityIdData.None, Circle(Vector2.zero), results);
            query.FillTargetIds(NetworkEntityIdData.None, Circle(Vector2.zero), results);

            Assert.That(results, Is.EqualTo(new[] { targetId, otherId }));
        }

        [Test]
        public void RemovingFakeServiceEntryIsObservedOnTheNextQuery()
        {
            var targetId = new NetworkEntityIdData(NetworkEntityType.Enemy, 1);
            var service = new TestGameEntityComponentService();
            service.Add(targetId, new TestHurtbox(true, Vector2.zero));
            var query = new CombatTargetQueryService(service);
            var results = new List<NetworkEntityIdData>();
            query.FillTargetIds(NetworkEntityIdData.None, Circle(Vector2.zero), results);
            Assert.That(results, Is.EqualTo(new[] { targetId }));

            service.RemoveEntity(targetId);
            results.Clear();
            query.FillTargetIds(NetworkEntityIdData.None, Circle(Vector2.zero), results);

            Assert.That(results, Is.Empty);
        }

        [Test]
        public void ExistingComponentServiceTraversesAllHurtboxesAndCurrentRegistryMembership()
        {
            var rootObject = new GameObject("Combat registry test root");
            try
            {
                var root = rootObject.AddComponent<TestEntityRoot>();
                var components = (ICollection<INetworkEntityComponent>)root.Components;
                components.Add(new TestHurtbox(true, new Vector2(20f, 0f)));
                components.Add(new TestHurtbox(true, Vector2.zero));
                components.Add(new TestHurtbox(true, Vector2.right));
                var registry = new NetworkEntityRegistry();
                var entityId = new NetworkEntityIdData(NetworkEntityType.Enemy, 1);
                // Seed membership without reading/writing Fusion state on an unspawned root.
                var membership = (IDictionary<NetworkEntityIdData, BaseNetworkEntityRoot>)registry.EntitiesById;
                membership.Add(entityId, root);
                var query = new CombatTargetQueryService(new GameEntityComponentService(registry));
                var results = new List<NetworkEntityIdData>();

                query.FillTargetIds(NetworkEntityIdData.None, Circle(Vector2.zero), results);
                Assert.That(results, Is.EqualTo(new[] { entityId }));

                membership.Remove(entityId);
                results.Clear();
                query.FillTargetIds(NetworkEntityIdData.None, Circle(Vector2.zero), results);
                Assert.That(results, Is.Empty);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void QueryReadsCurrentAvailabilityAndWorldPose()
        {
            var targetId = new NetworkEntityIdData(NetworkEntityType.Enemy, 1);
            var hurtbox = new TestHurtbox(false, Vector2.zero);
            var service = new TestGameEntityComponentService();
            service.Add(targetId, hurtbox);
            var query = new CombatTargetQueryService(service);
            var results = new List<NetworkEntityIdData>();
            query.FillTargetIds(NetworkEntityIdData.None, Circle(Vector2.zero), results);
            Assert.That(results, Is.Empty);

            hurtbox.IsAvailable = true;
            query.FillTargetIds(NetworkEntityIdData.None, Circle(Vector2.zero), results);
            Assert.That(results, Is.EqualTo(new[] { targetId }));

            hurtbox.WorldShape = Circle(new Vector2(20f, 0f));
            results.Clear();
            query.FillTargetIds(NetworkEntityIdData.None, Circle(Vector2.zero), results);
            Assert.That(results, Is.Empty);
        }

        [Test]
        public void QueryValidatesRequiredDependenciesAndResultCollection()
        {
            Assert.Throws<ArgumentNullException>(() => new CombatTargetQueryService(null));
            var query = new CombatTargetQueryService(new TestGameEntityComponentService());
            Assert.Throws<ArgumentNullException>(() => query.FillTargetIds(NetworkEntityIdData.None, Circle(Vector2.zero), null));
        }

        [Test]
        public void HurtboxWorldShapeRejectsAnUnspawnedEntityBeforeAndAfterInitialization()
        {
            var rootObject = new GameObject("Unspawned hurtbox test");
            try
            {
                var root = rootObject.AddComponent<TestEntityRoot>();
                var hurtbox = rootObject.AddComponent<CombatHurtboxComponent>();
                ConfigureHurtbox(hurtbox, root);
                Assert.Throws<InvalidOperationException>(() => { _ = hurtbox.WorldShape; });
                hurtbox.Init();
                Assert.That(hurtbox.IsInitialized, Is.True);
                Assert.That(hurtbox.HasValidNetworkContext, Is.False);
                Assert.That(hurtbox.IsComponentAvailable, Is.False);
                Assert.Throws<InvalidOperationException>(() => { _ = hurtbox.WorldShape; });
            }
            finally { UnityEngine.Object.DestroyImmediate(rootObject); }
        }

        [Test]
        public void HurtboxRequiresInitializationAndSpawnedObjectAndResetOnlyClearsLocalReadiness()
        {
            var rootObject = new GameObject("Combat test root");
            try
            {
                var root = rootObject.AddComponent<TestEntityRoot>();
                var hurtbox = rootObject.AddComponent<CombatHurtboxComponent>();
                ConfigureHurtbox(hurtbox, root);
                Assert.That(hurtbox.IsAvailable, Is.False);

                hurtbox.Init();

                // An injected but unspawned root must remain unavailable.
                Assert.That(hurtbox.IsAvailable, Is.False);
                Assert.That(hurtbox.IsInitialized, Is.True);
                hurtbox.enabled = false;
                Assert.That(hurtbox.IsAvailable, Is.False);
                hurtbox.enabled = true;
                rootObject.SetActive(false);
                Assert.That(hurtbox.IsAvailable, Is.False);

                Assert.DoesNotThrow(hurtbox.ResetLocalRuntime);
                Assert.That(hurtbox.IsInitialized, Is.False);
                Assert.That(hurtbox.IsAvailable, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(rootObject);
            }
        }

        [Test]
        public void FailedHurtboxInitializationDoesNotLeaveReadinessSet()
        {
            var rootObject = new GameObject("Combat test root");
            try
            {
                var hurtbox = rootObject.AddComponent<CombatHurtboxComponent>();
                Assert.Throws<InvalidOperationException>(hurtbox.Init);
                Assert.That(hurtbox.IsInitialized, Is.False);
                Assert.That(hurtbox.IsAvailable, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(rootObject);
            }
        }

        private static CombatShapeData Circle(Vector2 center)
        {
            return new CombatShapeData(CombatShapeType.Circle, center, Vector2.zero, 1f, 0f);
        }

        private static void ConfigureHurtbox(CombatHurtboxComponent hurtbox, TestEntityRoot root)
        {
            typeof(BaseNetworkEntityComponent).GetProperty(nameof(BaseNetworkEntityComponent.ParentNetworkBehaviour))
                .SetValue(hurtbox, root);
            typeof(CombatHurtboxComponent).GetField("_origin", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(hurtbox, root);
            typeof(CombatHurtboxComponent).GetField("_shape", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(hurtbox, new CombatShapeSettings(CombatShapeType.Circle, new Vector2(0f, 0.2f), 0.25f, Vector2.zero, 0f));
        }

        private sealed class TestHurtbox : ICombatHurtbox, INetworkEntityComponent
        {
            public TestHurtbox(bool isAvailable, Vector2 center)
            {
                IsAvailable = isAvailable;
                WorldShape = Circle(center);
            }

            public bool IsAvailable { get; set; }
            public bool IsInitialized => true;
            public bool HasValidNetworkContext => true;
            public bool HasSimulationAuthority => true;
            public bool IsComponentAvailable => IsAvailable;
            public CombatShapeData WorldShape { get; set; }
            public IReadOnlyList<INetworkEntityComponent> Components => Array.Empty<INetworkEntityComponent>();

            public void Init() { }

            public void NetworkTick() { }

            public void ClientRender() { }

            public void ResetLocalRuntime() { }
        }

        private sealed class TestGameEntityComponentService : IGameEntityComponentService
        {
            private readonly List<KeyValuePair<NetworkEntityIdData, ICombatHurtbox>> _hurtboxes = new();

            public void Add(NetworkEntityIdData entityId, ICombatHurtbox hurtbox)
            {
                _hurtboxes.Add(new KeyValuePair<NetworkEntityIdData, ICombatHurtbox>(entityId, hurtbox));
            }

            public void RemoveEntity(NetworkEntityIdData entityId)
            {
                _hurtboxes.RemoveAll(entry => entry.Key == entityId);
            }

            public void FillEntityIds<TComponent>(Predicate<TComponent> predicate, ICollection<NetworkEntityIdData> results)
                where TComponent : class
            {
                foreach (KeyValuePair<NetworkEntityIdData, ICombatHurtbox> entry in _hurtboxes)
                {
                    if (entry.Value is TComponent component && predicate(component))
                    {
                        results.Add(entry.Key);
                    }
                }
            }

            public bool TryFindEntityId<TComponent>(Predicate<TComponent> predicate, out NetworkEntityIdData entityId)
                where TComponent : class
            {
                throw new NotSupportedException();
            }

            public void FillEntityIdsSortedBy<TComponent, TSortKey>(Func<TComponent, TSortKey> sortKeySelector, ICollection<NetworkEntityIdData> results)
                where TComponent : class
            {
                throw new NotSupportedException();
            }

            public bool TryFindFirstEntityIdSortedBy<TComponent, TSortKey>(Func<TComponent, TSortKey> sortKeySelector, out NetworkEntityIdData entityId)
                where TComponent : class
            {
                throw new NotSupportedException();
            }

            public void ApplyToAll<TComponent>(Action<TComponent> action) where TComponent : class
            {
                throw new NotSupportedException();
            }
        }

        public sealed class TestEntityRoot : BaseNetworkEntityRoot
        {
            protected override IEnumerable<INetworkEntityComponent> CreateComponents()
            {
                return Array.Empty<INetworkEntityComponent>();
            }
        }
    }
}
