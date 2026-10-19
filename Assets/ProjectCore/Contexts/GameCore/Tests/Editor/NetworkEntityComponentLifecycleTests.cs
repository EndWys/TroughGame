using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ProjectCore.GameCore
{
    public sealed class NetworkEntityComponentLifecycleTests
    {
        [Test]
        public void ReadinessIsPublishedOnlyAfterInitializationAndClearedBeforeReset()
        {
            var lifecycle = new NetworkEntityComponentLifecycleProcessor();
            int resets = 0;
            Action reset = () =>
            {
                Assert.That(lifecycle.IsInitialized, Is.False);
                resets++;
            };
            lifecycle.Initialize(() => Assert.That(lifecycle.IsInitialized, Is.False), reset);
            Assert.That(lifecycle.IsInitialized, Is.True);
            lifecycle.Reset(reset);
            lifecycle.Reset(reset);
            Assert.That(lifecycle.IsInitialized, Is.False);
            Assert.That(resets, Is.EqualTo(3));
        }

        [Test]
        public void FailedInitializationCleansPartialStateAndPreservesOriginalError()
        {
            var lifecycle = new NetworkEntityComponentLifecycleProcessor();
            var expected = new InvalidOperationException("Invalid authored dependency");
            int partialState = 0;
            int resets = 0;
            var actual = Assert.Throws<InvalidOperationException>(() => lifecycle.Initialize(() =>
            {
                partialState = 5;
                throw expected;
            }, () => { partialState = 0; resets++; }));
            Assert.That(actual, Is.SameAs(expected));
            Assert.That(partialState, Is.Zero);
            Assert.That(resets, Is.EqualTo(2));
            Assert.That(lifecycle.IsInitialized, Is.False);
        }

        [Test]
        public void FailedInitializationReportsCleanupFailureWithoutLosingCause()
        {
            var lifecycle = new NetworkEntityComponentLifecycleProcessor();
            var initializeError = new InvalidOperationException("Initialize");
            var cleanupError = new InvalidOperationException("Cleanup");
            int resets = 0;
            var result = Assert.Throws<AggregateException>(() => lifecycle.Initialize(
                () => throw initializeError,
                () => { if (++resets > 1) throw cleanupError; }));
            Assert.That(result.InnerExceptions, Is.EqualTo(new[] { initializeError, cleanupError }));
            Assert.That(lifecycle.IsInitialized, Is.False);
        }

        [Test]
        public void NetworkContextValidityIsIndependentOfSuccessfulInitialization()
        {
            var lifecycle = new NetworkEntityComponentLifecycleProcessor();
            Assert.That(lifecycle.HasValidNetworkContext(null), Is.False);
            lifecycle.Initialize(() => { }, () => { });
            Assert.That(lifecycle.IsInitialized, Is.True);
            Assert.That(lifecycle.HasValidNetworkContext(null), Is.False);
        }

        [Test]
        public void MonoBehaviourWrapperGatesUninitializedAndInvalidCalls()
        {
            var owner = new GameObject("Lifecycle test");
            try
            {
                var component = owner.AddComponent<TestComponent>();
                Assert.That(component.IsComponentAvailable, Is.False);
                component.NetworkTick();
                component.ClientRender();
                Assert.That(component.Invalidations, Is.Zero);
                component.Init();
                Assert.That(component.IsInitialized, Is.True);
                component.NetworkTick();
                component.ClientRender();
                Assert.That(component.Ticks, Is.Zero);
                Assert.That(component.Renders, Is.Zero);
                Assert.That(component.Invalidations, Is.EqualTo(2));
                component.ResetLocalRuntime();
                component.ResetLocalRuntime();
                Assert.That(component.IsInitialized, Is.False);
                component.FailInitialization = true;
                Assert.Throws<InvalidOperationException>(component.Init);
                Assert.That(component.IsInitialized, Is.False);
                component.NetworkTick();
                component.ClientRender();
                Assert.That(component.Ticks, Is.Zero);
                Assert.That(component.Renders, Is.Zero);
            }
            finally { Object.DestroyImmediate(owner); }
        }

        [Test]
        public void NetworkBehaviourWrapperUsesTheSameReadinessPolicy()
        {
            var owner = new GameObject("Network lifecycle test");
            try
            {
                var component = owner.AddComponent<TestBehaviourComponent>();
                Assert.That(component.IsComponentAvailable, Is.False);
                component.NetworkTick();
                component.ClientRender();
                Assert.That(component.Invalidations, Is.Zero);
                component.Init();
                Assert.That(component.IsInitialized, Is.True);
                component.NetworkTick();
                component.ClientRender();
                Assert.That(component.Ticks, Is.Zero);
                Assert.That(component.Renders, Is.Zero);
                Assert.That(component.Invalidations, Is.EqualTo(2));
                component.ResetLocalRuntime();
                component.ResetLocalRuntime();
                Assert.That(component.IsInitialized, Is.False);
            }
            finally { Object.DestroyImmediate(owner); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void CompositeResetsEveryChildInReverseOrderEvenWhenOneResetFails(bool cleanupFails)
        {
            var owner = new GameObject("Composite lifecycle test");
            try
            {
                var resets = new List<int>();
                var composite = owner.AddComponent<TestComposite>();
                composite.Children = new INetworkEntityComponent[]
                {
                    new TestChild(1, resets),
                    new TestChild(2, resets),
                    new TestChild(3, resets),
                };
                composite.Init();
                if (cleanupFails) ((TestChild)composite.Children[1]).ResetError = new InvalidOperationException();
                if (cleanupFails) Assert.Throws<AggregateException>(composite.ResetLocalRuntime);
                else Assert.DoesNotThrow(composite.ResetLocalRuntime);
                Assert.That(resets, Is.EqualTo(new[] { 3, 2, 1 }));
                Assert.That(composite.IsInitialized, Is.False);
                Assert.That(composite.Components, Is.Empty);
                Assert.DoesNotThrow(composite.ResetLocalRuntime);
            }
            finally { Object.DestroyImmediate(owner); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RootInitializationFailureRollsBackChildrenAndPreventsDispatch(bool failAfterChildren)
        {
            var owner = new GameObject("Root lifecycle test");
            try
            {
                var resets = new List<int>();
                var first = new TestChild(1, resets);
                var second = new TestChild(2, resets) { FailInitialization = !failAfterChildren };
                var last = new TestChild(3, resets);
                var root = owner.AddComponent<TestRoot>();
                root.Children = new INetworkEntityComponent[] { first, second, last };
                root.FailAfterChildren = failAfterChildren;
                Assert.Throws<InvalidOperationException>(root.Spawned);
                root.FixedUpdateNetwork();
                root.Render();
                Assert.That(resets, Is.EqualTo(new[] { 3, 2, 1 }));
                Assert.That(first.Dispatches + second.Dispatches + last.Dispatches, Is.Zero);
                Assert.That(root.Components, Is.Empty);
            }
            finally { Object.DestroyImmediate(owner); }
        }

        [Test]
        public void MovementOriginAndCollisionCenterApplyOffsetExactlyOnce()
        {
            var owner = new GameObject("Movement origin test");
            try
            {
                owner.transform.position = new Vector3(3f, 4f, 10f);
                owner.AddComponent<TestRoot>();
                var body = owner.AddComponent<TransformMovementBodyComponent>();
                typeof(TransformMovementBodyComponent).GetField("_position", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                    .SetValue(body, owner.GetComponent<IEntityPositionAccessor>());
                Assert.That(owner.GetComponent<IEntityPositionAccessor>().Position,
                    Is.EqualTo(new Vector2(3f, 4f)));
                Assert.That(body.Position, Is.EqualTo(owner.GetComponent<IEntityPositionAccessor>().Position + body.Offset));
                owner.GetComponent<IMovementBodyVelocityMutator>().Velocity = Vector2.one;
                body.ResetLocalRuntime();
                Assert.That(body.Velocity, Is.EqualTo(Vector2.zero));
                Assert.That(owner.GetComponent<IEntityPositionAccessor>().Position, Is.EqualTo(new Vector2(3f, 4f)));
            }
            finally { Object.DestroyImmediate(owner); }
        }

        public sealed class TestComponent : BaseNetworkEntityComponent
        {
            public bool FailInitialization;
            public int Ticks;
            public int Renders;
            public int Invalidations;
            protected override void OnNetworkContextInvalidated() { Invalidations++; }
            protected override void OnInitialize()
            {
                if (FailInitialization) throw new InvalidOperationException();
            }
            protected override void OnNetworkTick() { Ticks++; }
            protected override void OnClientRender() { Renders++; }
        }

        public sealed class TestBehaviourComponent : BaseNetworkEntityBehaviourComponent
        {
            public int Ticks;
            public int Renders;
            public int Invalidations;
            protected override void OnNetworkContextInvalidated() { Invalidations++; }
            protected override void OnNetworkTick() { Ticks++; }
            protected override void OnClientRender() { Renders++; }
        }

        public sealed class TestComposite : BaseNetworkEntityCompositeComponent
        {
            public INetworkEntityComponent[] Children;
            protected override IEnumerable<INetworkEntityComponent> CreateComponents() => Children;
        }

        public sealed class TestRoot : BaseNetworkEntityRoot
        {
            public INetworkEntityComponent[] Children;
            public bool FailAfterChildren;
            protected override IEnumerable<INetworkEntityComponent> CreateComponents() => Children;
            protected override void AfterComponentsInitialized()
            {
                if (FailAfterChildren) throw new InvalidOperationException();
            }
        }

        private sealed class TestChild : INetworkEntityComponent
        {
            private readonly int _id;
            private readonly List<int> _resets;
            public bool FailInitialization;
            public Exception ResetError;
            public int Dispatches;
            public bool IsInitialized { get; private set; }
            public bool HasValidNetworkContext => true;
            public bool HasSimulationAuthority => true;
            public bool IsComponentAvailable => IsInitialized;
            public TestChild(int id, List<int> resets) { _id = id; _resets = resets; }
            public IReadOnlyList<INetworkEntityComponent> Components => Array.Empty<INetworkEntityComponent>();
            public void Init() { if (FailInitialization) throw new InvalidOperationException(); IsInitialized = true; }
            public void NetworkTick() { Dispatches++; }
            public void ClientRender() { Dispatches++; }
            public void ResetLocalRuntime()
            {
                IsInitialized = false;
                _resets.Add(_id);
                if (ResetError != null) throw ResetError;
            }
        }
    }
}
