using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace ProjectCore.GameCore
{
    public sealed class LocalInputReaderTests
    {
        private InputActionAsset _inputActions;
        private LocalInputProvider _provider;
        private Keyboard _keyboard;
        private InputSettings _originalInputSettings;
        private InputSettings _testInputSettings;

        [SetUp]
        public void SetUp()
        {
            _originalInputSettings = InputSystem.settings;
            _testInputSettings = UnityEngine.Object.Instantiate(_originalInputSettings);
            InputSystem.settings = _testInputSettings;
            _testInputSettings.SetInternalFeatureFlag("RUN_PLAYER_UPDATES_IN_EDIT_MODE", true);
            _inputActions = ScriptableObject.CreateInstance<InputActionAsset>();
            InputActionMap gameplayMap = _inputActions.AddActionMap("Gameplay");
            gameplayMap.AddAction("Move", InputActionType.Value);
            gameplayMap.AddAction("Dodge", InputActionType.Button);

            _provider = new LocalInputProvider();
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                _provider?.Dispose();
                if (_keyboard != null)
                {
                    InputSystem.RemoveDevice(_keyboard);
                    _keyboard = null;
                }
                UnityEngine.Object.DestroyImmediate(_inputActions);
            }
            finally
            {
                if (_originalInputSettings != null)
                    InputSystem.settings = _originalInputSettings;
                if (_testInputSettings != null)
                    UnityEngine.Object.DestroyImmediate(_testInputSettings);
            }
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

        [TestCase(true)]
        [TestCase(false)]
        public void PressSequenceFollowsCallbacksRatherThanActionRegistrationOrder(bool attackFirst)
        {
            InitializeKeyboardActions();
            Key firstKey = attackFirst ? Key.A : Key.D;
            Key secondKey = attackFirst ? Key.D : Key.A;
            PressKeys(firstKey);
            PressKeys(firstKey, secondKey);
            _provider.Capture();

            LocalButtonStateData attack = _provider.ReadButton("Gameplay/Attack");
            LocalButtonStateData dodge = _provider.ReadButton("Gameplay/Dodge");
            Assert.That(attack.WasPressed, Is.True);
            Assert.That(dodge.WasPressed, Is.True);
            Assert.That(attack.PressSequence, Is.GreaterThan(0ul));
            Assert.That(attackFirst ? dodge.PressSequence : attack.PressSequence,
                Is.GreaterThan(attackFirst ? attack.PressSequence : dodge.PressSequence));
            Assert.That(_provider.ReadButton("Gameplay/Attack"), Is.EqualTo(attack));
        }

        [Test]
        public void HeldButtonAndCaptureDoNotInventAnotherPressOrRefreshItsSequence()
        {
            InitializeKeyboardActions();
            PressKeys(Key.A);
            _provider.Capture();
            LocalButtonStateData pressed = _provider.ReadButton("Gameplay/Attack");
            Assert.That(pressed.WasPressed, Is.True);

            PressKeys(Key.A);
            _provider.Capture();
            LocalButtonStateData held = _provider.ReadButton("Gameplay/Attack");
            Assert.That(held.IsHeld, Is.True);
            Assert.That(held.WasPressed, Is.False);
            Assert.That(held.PressSequence, Is.EqualTo(pressed.PressSequence));

            PressKeys();
            _provider.Capture();
            Assert.That(_provider.ReadButton("Gameplay/Attack").WasReleased, Is.True);
            PressKeys(Key.A);
            _provider.Capture();
            LocalButtonStateData repeated = _provider.ReadButton("Gameplay/Attack");
            Assert.That(repeated.WasPressed, Is.True);
            Assert.That(repeated.PressSequence, Is.GreaterThan(pressed.PressSequence));
        }

        [Test]
        public void LatestRepeatedPressBeforeCaptureWinsOverAnEarlierOtherButton()
        {
            InitializeKeyboardActions();
            PressKeys(Key.A);
            PressKeys();
            PressKeys(Key.D);
            PressKeys(Key.D, Key.A);
            _provider.Capture();

            LocalButtonStateData attack = _provider.ReadButton("Gameplay/Attack");
            LocalButtonStateData dodge = _provider.ReadButton("Gameplay/Dodge");
            Assert.That(attack.WasPressed, Is.True);
            Assert.That(attack.WasReleased, Is.True);
            Assert.That(attack.PressSequence, Is.GreaterThan(dodge.PressSequence));
        }

        [Test]
        public void LegacyButtonStateConstructorRemainsCompatible()
        {
            var state = new LocalButtonStateData(true, true, false, true);
            Assert.That(state.IsHeld, Is.True);
            Assert.That(state.WasPressed, Is.True);
            Assert.That(state.WasReleased, Is.False);
            Assert.That(state.WasPerformed, Is.True);
            Assert.That(state.PressSequence, Is.Zero);
        }

        private void InitializeKeyboardActions()
        {
            _keyboard = InputSystem.AddDevice<Keyboard>();
            InputActionMap map = _inputActions.FindActionMap("Gameplay", true);
            map.FindAction("Dodge", true).AddBinding("<Keyboard>/d");
            map.AddAction("Attack", InputActionType.Button, "<Keyboard>/a");
            _inputActions.devices = new InputDevice[] { _keyboard };
            _provider.Initialize(_inputActions);
        }

        private void PressKeys(params Key[] keys)
        {
            InputSystem.QueueStateEvent(_keyboard, new KeyboardState(keys));
            InputSystem.Update();
        }
    }
}
