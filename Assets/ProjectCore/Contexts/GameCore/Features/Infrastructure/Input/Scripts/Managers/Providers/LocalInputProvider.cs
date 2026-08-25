using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace ProjectCore.GameCore
{
    public sealed class LocalInputProvider : ILocalInputReader, IDisposable
    {
        private readonly Dictionary<string, InputAction> _actions =
            new Dictionary<string, InputAction>(StringComparer.Ordinal);
        private readonly Dictionary<Guid, ButtonInputState> _buttonStates =
            new Dictionary<Guid, ButtonInputState>();

        private InputActionAsset _inputActions;
        private bool _hasCapturedSample;

        public void Initialize(InputActionAsset inputActions)
        {
            if (inputActions == null)
            {
                throw new ArgumentNullException(nameof(inputActions));
            }

            if (_inputActions != null)
            {
                throw new InvalidOperationException(
                    "LocalInputProvider is already initialized.");
            }

            _inputActions = inputActions;

            foreach (InputActionMap actionMap in _inputActions.actionMaps)
            {
                foreach (InputAction action in actionMap.actions)
                {
                    string actionPath = GetActionPath(action);
                    if (!_actions.TryAdd(actionPath, action))
                    {
                        throw new InvalidOperationException(
                            $"Input action path '{actionPath}' is duplicated.");
                    }

                    _buttonStates.Add(action.id, new ButtonInputState());
                    action.started += OnActionStarted;
                    action.performed += OnActionPerformed;
                    action.canceled += OnActionCanceled;
                }
            }

            _inputActions.Enable();
        }

        public void Capture()
        {
            EnsureInitialized();

            foreach (InputAction action in _actions.Values)
            {
                ButtonInputState buttonState = _buttonStates[action.id];
                buttonState.CapturedState = new LocalButtonStateData(
                    action.IsPressed(),
                    buttonState.WasPressed,
                    buttonState.WasReleased,
                    buttonState.WasPerformed);
                buttonState.ResetTransitions();
            }

            _hasCapturedSample = true;
        }

        public T ReadValue<T>(string actionPath) where T : struct
        {
            return GetAction(actionPath).ReadValue<T>();
        }

        public LocalButtonStateData ReadButton(string actionPath)
        {
            if (!_hasCapturedSample)
            {
                throw new InvalidOperationException(
                    "LocalInputProvider must capture a sample before reading buttons.");
            }

            InputAction action = GetAction(actionPath);
            return _buttonStates[action.id].CapturedState;
        }

        public void Dispose()
        {
            if (_inputActions == null)
            {
                return;
            }

            foreach (InputAction action in _actions.Values)
            {
                action.started -= OnActionStarted;
                action.performed -= OnActionPerformed;
                action.canceled -= OnActionCanceled;
            }

            _inputActions.Disable();
            _buttonStates.Clear();
            _actions.Clear();
            _inputActions = null;
            _hasCapturedSample = false;
        }

        private InputAction GetAction(string actionPath)
        {
            if (string.IsNullOrWhiteSpace(actionPath))
            {
                throw new ArgumentException(
                    "Input action path must not be empty.",
                    nameof(actionPath));
            }

            if (_inputActions == null)
            {
                throw new InvalidOperationException(
                    "LocalInputProvider is not initialized.");
            }

            return _actions.TryGetValue(actionPath, out InputAction action)
                ? action
                : throw new KeyNotFoundException(
                    $"Input action '{actionPath}' is not registered.");
        }

        private void EnsureInitialized()
        {
            if (_inputActions == null)
            {
                throw new InvalidOperationException(
                    "LocalInputProvider is not initialized.");
            }
        }

        private void OnActionStarted(InputAction.CallbackContext context)
        {
            _buttonStates[context.action.id].WasPressed = true;
        }

        private void OnActionPerformed(InputAction.CallbackContext context)
        {
            _buttonStates[context.action.id].WasPerformed = true;
        }

        private void OnActionCanceled(InputAction.CallbackContext context)
        {
            _buttonStates[context.action.id].WasReleased = true;
        }

        private static string GetActionPath(InputAction action)
        {
            return $"{action.actionMap.name}/{action.name}";
        }

        private sealed class ButtonInputState
        {
            public bool WasPressed { get; set; }
            public bool WasReleased { get; set; }
            public bool WasPerformed { get; set; }
            public LocalButtonStateData CapturedState { get; set; }

            public void ResetTransitions()
            {
                WasPressed = false;
                WasReleased = false;
                WasPerformed = false;
            }
        }
    }
}
