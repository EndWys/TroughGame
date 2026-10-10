using System;
using System.Reflection;
using System.Threading;
using Fusion;
using NUnit.Framework;
using ProjectCore.Template;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerSpawnConfigurationTests
    {
        private GameObject _owner;
        private PlayerFeature _feature;
        private PlayerSpawnComponent _spawn;

        [SetUp]
        public void SetUp()
        {
            _owner = new GameObject("PlayerSpawnValidationTest");
            _feature = _owner.AddComponent<PlayerFeature>();
            _spawn = _owner.AddComponent<PlayerSpawnComponent>();
            SetField(_feature, "_entitiesContainer", _owner.transform);
            SetField(_feature, "_spawnComponent", _spawn);
            SetField(_spawn, "_playerPrefab", (NetworkPrefabRef)Guid.NewGuid());
            SetField(_spawn, "_spawnPoints", new[] { _owner.transform });
            _feature.InstallBindings(new DiContainer());
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(_owner);
        }

        [Test]
        public void InitializationRejectsMissingSpawnComponent()
        {
            SetField<PlayerFeature, PlayerSpawnComponent>(_feature, "_spawnComponent", null);
            AssertInitializationFails("PlayerFeature._spawnComponent");
        }

        [Test]
        public void InitializationRejectsMissingPrefab()
        {
            SetField(_spawn, "_playerPrefab", default(NetworkPrefabRef));
            AssertInitializationFails("_playerPrefab");
        }

        [TestCase(true)]
        [TestCase(false)]
        public void InitializationRejectsMissingSpawnPoints(bool isNull)
        {
            SetField(_spawn, "_spawnPoints", isNull ? null : Array.Empty<Transform>());
            AssertInitializationFails("_spawnPoints");
        }

        [Test]
        public void InitializationRejectsMissingSecondPointBeforeAnyPlayerJoins()
        {
            SetField(_spawn, "_spawnPoints", new[] { _owner.transform, null });
            InvalidOperationException exception = AssertInitializationFails("_spawnPoints[1]");
            Assert.That(exception.Message, Does.Contain(_owner.name));
        }

        [Test]
        public void ValidConfigurationInitializesWithoutRunningNetworkSession()
        {
            Assert.DoesNotThrow(InitializeFeature);
        }

        private InvalidOperationException AssertInitializationFails(string field)
        {
            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(InitializeFeature);
            Assert.That(exception.Message, Does.Contain(field));
            return exception;
        }

        private void InitializeFeature()
        {
            ((IBaseFeature)_feature).InitializeAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        private static void SetField<TTarget, TValue>(TTarget target, string field, TValue value)
        {
            typeof(TTarget).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(target, value);
        }
    }
}
