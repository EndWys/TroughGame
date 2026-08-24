using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectCore.GameCore
{
    public sealed class LocalInputAccumulatorTests
    {
        private InputActionAsset _inputActions;
        private LocalInputAccumulator _accumulator;

        [SetUp]
        public void SetUp()
        {
            _inputActions = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap gameplayMap = _inputActions.AddActionMap("Gameplay");
            gameplayMap.AddAction("Move", InputActionType.Value);

            _accumulator = new LocalInputAccumulator();
        }

        [TearDown]
        public void TearDown()
        {
            _accumulator.Dispose();
            UnityEngine.Object.DestroyImmediate(_inputActions);
        }

        [Test]
        public void InitializeEnablesConfiguredActions()
        {
            _accumulator.Initialize(_inputActions);

            Assert.That(_inputActions.enabled, Is.True);
            Assert.That(
                _accumulator.ReadValue<Vector2>("Gameplay/Move"),
                Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ReadValueRejectsUnknownAction()
        {
            _accumulator.Initialize(_inputActions);

            Assert.Throws<KeyNotFoundException>(() =>
                _accumulator.ReadValue<Vector2>("Gameplay/Aim"));
        }

        [Test]
        public void InitializeRejectsSecondInitialization()
        {
            _accumulator.Initialize(_inputActions);

            Assert.Throws<InvalidOperationException>(() =>
                _accumulator.Initialize(_inputActions));
        }

        [Test]
        public void DisposeDisablesConfiguredActions()
        {
            _accumulator.Initialize(_inputActions);

            _accumulator.Dispose();

            Assert.That(_inputActions.enabled, Is.False);
        }
    }
}
