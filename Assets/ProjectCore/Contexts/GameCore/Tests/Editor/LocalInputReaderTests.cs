using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectCore.GameCore
{
    public sealed class LocalInputReaderTests
    {
        private InputActionAsset _inputActions;
        private LocalInputProvider _provider;

        [SetUp]
        public void SetUp()
        {
            _inputActions = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap gameplayMap = _inputActions.AddActionMap("Gameplay");
            gameplayMap.AddAction("Move", InputActionType.Value);
            gameplayMap.AddAction("Dodge", InputActionType.Button);

            _provider = new LocalInputProvider();
        }

        [TearDown]
        public void TearDown()
        {
            _provider.Dispose();
            UnityEngine.Object.DestroyImmediate(_inputActions);
        }

        [Test]
        public void InitializeEnablesConfiguredActions()
        {
            _provider.Initialize(_inputActions);

            Assert.That(_inputActions.enabled, Is.True);
            Assert.That(
                _provider.ReadValue<Vector2>("Gameplay/Move"),
                Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ReadValueRejectsUnknownAction()
        {
            _provider.Initialize(_inputActions);

            Assert.Throws<KeyNotFoundException>(() =>
                _provider.ReadValue<Vector2>("Gameplay/Aim"));
        }

        [Test]
        public void CaptureMakesButtonStateReadableWithoutConsumingIt()
        {
            _provider.Initialize(_inputActions);
            _provider.Capture();

            LocalButtonStateData firstRead = _provider.ReadButton("Gameplay/Dodge");
            LocalButtonStateData secondRead = _provider.ReadButton("Gameplay/Dodge");

            Assert.That(firstRead.IsHeld, Is.False);
            Assert.That(firstRead.WasPressed, Is.False);
            Assert.That(secondRead.WasReleased, Is.False);
        }

        [Test]
        public void InitializeRejectsSecondInitialization()
        {
            _provider.Initialize(_inputActions);

            Assert.Throws<InvalidOperationException>(() =>
                _provider.Initialize(_inputActions));
        }

        [Test]
        public void DisposeDisablesConfiguredActions()
        {
            _provider.Initialize(_inputActions);

            _provider.Dispose();

            Assert.That(_inputActions.enabled, Is.False);
        }
    }
}
