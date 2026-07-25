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

            var flow = new FeatureInitializationFlow(features, new DiContainer());

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

            public UniTask InitializeAsync(
                DiContainer container,
                CancellationToken cancellationToken)
            {
                _events.Add(_name);
                return UniTask.CompletedTask;
            }
        }
    }
}
