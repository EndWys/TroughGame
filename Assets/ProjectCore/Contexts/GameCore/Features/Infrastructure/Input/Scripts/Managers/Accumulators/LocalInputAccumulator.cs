using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace ProjectCore.GameCore
{
    public sealed class LocalInputAccumulator : ILocalInputAccumulator, IDisposable
    {
        private readonly Dictionary<string, InputAction> _actions =
            new Dictionary<string, InputAction>(StringComparer.Ordinal);
        private readonly Dictionary<Guid, ButtonTransitions> _buttonTransitions =
            new Dictionary<Guid, ButtonTransitions>();

        private InputActionAsset _inputActions;

        public void Initialize(InputActionAsset inputActions)
        {
            if (inputActions == null)
            {
                throw new ArgumentNullException(nameof(inputActions));
            }

            if (_inputActions != null)
            {
                throw new InvalidOperationException(
                    "LocalInputAccumulator is already initialized.");
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

                    _buttonTransitions.Add(action.id, new ButtonTransitions());
                    action.started += OnActionStarted;
                    action.performed += OnActionPerformed;
                    action.canceled += OnActionCanceled;
                }
            }

            _inputActions.Enable();
        }

        public T ReadValue<T>(string actionPath) where T : struct
        {
            return GetAction(actionPath).ReadValue<T>();
        }

        public LocalButtonInputData ConsumeButton(string actionPath)
        {
            InputAction action = GetAction(actionPath);
            ButtonTransitions transitions = _buttonTransitions[action.id];
            var inputData = new LocalButtonInputData(
                action.IsPressed(),
                transitions.WasPressed,
                transitions.WasReleased,
                transitions.WasPerformed);

            transitions.Reset();
            return inputData;
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
            _buttonTransitions.Clear();
            _actions.Clear();
            _inputActions = null;
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
                    "LocalInputAccumulator is not initialized.");
            }

            return _actions.TryGetValue(actionPath, out InputAction action)
                ? action
                : throw new KeyNotFoundException(
                    $"Input action '{actionPath}' is not registered.");
        }

        private void OnActionStarted(InputAction.CallbackContext context)
        {
            _buttonTransitions[context.action.id].WasPressed = true;
        }

        private void OnActionPerformed(InputAction.CallbackContext context)
        {
            _buttonTransitions[context.action.id].WasPerformed = true;
        }

        private void OnActionCanceled(InputAction.CallbackContext context)
        {
            _buttonTransitions[context.action.id].WasReleased = true;
        }

        private static string GetActionPath(InputAction action)
        {
            return $"{action.actionMap.name}/{action.name}";
        }

        private sealed class ButtonTransitions
        {
            public bool WasPressed { get; set; }
            public bool WasReleased { get; set; }
            public bool WasPerformed { get; set; }

            public void Reset()
            {
                WasPressed = false;
                WasReleased = false;
                WasPerformed = false;
            }
        }
    }
}
