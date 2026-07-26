using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Zenject;

namespace ProjectCore.Template
{
    public sealed class FeatureInitializationFlowTests
    {
        [Test]
        public void InitializesFeaturesInOrderExactlyOnce()
        {
            var events = new List<string>();
            var features = new List<IBaseFeature>
            {
                new TestFeature("First", events),
                new TestFeature("Second", events)
            };

            var flow = new FeatureInitializationFlow(features);

            flow.InitializeFeaturesAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult();
            flow.InitializeFeaturesAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            CollectionAssert.AreEqual(
                new[] { "First", "Second" },
                events);
        }

        [Test]
        public void FeatureGroupInstallsAndInitializesNestedFeaturesInDeclarationOrder()
        {
            var events = new List<string>();
            OrderedTestFeature.SetEvents(events);
            var group = new TestFeatureGroup();
            var container = new DiContainer();

            group.InstallBindings(container);
            group.InitializeAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            CollectionAssert.AreEqual(
                new[]
                {
                    "Install.First",
                    "Install.Second",
                    "Initialize.First",
                    "Initialize.Second"
                },
                events);
        }

        [Test]
        public void BaseFeatureResolvesConcreteServiceWithoutExpandingItsPublicContract()
        {
            // The service interface intentionally has no initialization method.
            // The feature owns initialization order and reaches the concrete
            // implementation only inside its explicit lifecycle method.
            var events = new List<string>();
            BaseFeatureTestService.SetEvents(events);
            var feature = new BaseFeatureTestFeature();
            var container = new DiContainer();

            feature.InstallBindings(container);
            ((IBaseFeature)feature).InitializeAsync(CancellationToken.None)
                .GetAwaiter()
                .GetResult();

            CollectionAssert.AreEqual(new[] { "Service.Initialize" }, events);
        }

        private sealed class TestFeature : IBaseFeature
        {
            private readonly string _name;
            private readonly List<string> _events;

            public TestFeature(string name, List<string> events)
            {
                _name = name;
                _events = events;
            }

            public void InstallBindings(DiContainer container)
            {
            }

            public UniTask InitializeAsync(CancellationToken cancellationToken)
            {
                _events.Add(_name);
                return UniTask.CompletedTask;
            }
        }

        private sealed class TestFeatureGroup : BaseFeatureGroup
        {
            protected override void AddFeatures()
            {
                AddFeature<FirstTestFeature>();
                AddFeature<SecondTestFeature>();
            }

            private sealed class FirstTestFeature : OrderedTestFeature
            {
                public FirstTestFeature()
                    : base("First")
                {
                }
            }

            private sealed class SecondTestFeature : OrderedTestFeature
            {
                public SecondTestFeature()
                    : base("Second")
                {
                }
            }
        }

        private abstract class OrderedTestFeature : IBaseFeature
        {
            private static List<string> _events;
            private readonly string _name;

            protected OrderedTestFeature(string name)
            {
                _name = name;
            }

            public static void SetEvents(List<string> events)
            {
                _events = events;
            }

            public void InstallBindings(DiContainer container)
            {
                _events.Add($"Install.{_name}");
            }

            public UniTask InitializeAsync(CancellationToken cancellationToken)
            {
                _events.Add($"Initialize.{_name}");
                return UniTask.CompletedTask;
            }
        }

        private interface IBaseFeatureTestService
        {
        }

        private sealed class BaseFeatureTestFeature : BaseFeature
        {
            protected override void InstallBindings()
            {
                BindAsSingle<IBaseFeatureTestService, BaseFeatureTestService>();
            }

            protected override UniTask InitializeAsync(CancellationToken cancellationToken)
            {
                return ResolveAs<IBaseFeatureTestService, BaseFeatureTestService>()
                    .InitializeAsync(cancellationToken);
            }
        }

        private sealed class BaseFeatureTestService : IBaseFeatureTestService
        {
            private static List<string> _events;

            public static void SetEvents(List<string> events)
            {
                _events = events;
            }

            public UniTask InitializeAsync(CancellationToken cancellationToken)
            {
                _events.Add("Service.Initialize");
                return UniTask.CompletedTask;
            }
        }
    }
}
