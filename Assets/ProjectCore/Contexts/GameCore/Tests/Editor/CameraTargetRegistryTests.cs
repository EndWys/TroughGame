using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class CameraTargetRegistryTests
    {
        [Test]
        public void RegisterMakesTargetCurrentAndPublishesChange()
        {
            var registry = new CameraTargetRegistry();
            var target = new TestCameraTarget();
            var changes = new List<ICameraTarget>();
            registry.TargetChanged += changes.Add;

            registry.Register(target);

            Assert.That(registry.CurrentTarget, Is.SameAs(target));
            Assert.That(changes, Has.Count.EqualTo(1));
            Assert.That(changes[0], Is.SameAs(target));
        }

        [Test]
        public void UnregisterClearsCurrentTargetAndPublishesNull()
        {
            var registry = new CameraTargetRegistry();
            var target = new TestCameraTarget();
            ICameraTarget changedTarget = target;
            registry.TargetChanged += value => changedTarget = value;
            registry.Register(target);

            registry.Unregister(target);

            Assert.That(registry.CurrentTarget, Is.Null);
            Assert.That(changedTarget, Is.Null);
        }

        private sealed class TestCameraTarget : ICameraTarget
        {
            public Transform TargetTransform => null;
        }
    }
}
