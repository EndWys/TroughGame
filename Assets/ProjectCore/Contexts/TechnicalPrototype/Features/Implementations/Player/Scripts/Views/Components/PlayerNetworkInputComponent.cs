using System;
using Fusion;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerNetworkInputComponent : BaseNetworkCallbacksBehaviour
    {
        private ILocalInputReader _localInputReader;

        [Inject]
        private void Construct(ILocalInputReader localInputReader)
        {
            _localInputReader = localInputReader ?? throw new ArgumentNullException(nameof(localInputReader));
        }

        public override void OnInput(NetworkRunner runner, NetworkInput input)
        {
            _localInputReader.Capture();

            Vector2 moveDirection = _localInputReader.ReadValue<Vector2>(PlayerInputActionConstants.Move);
            LocalButtonStateData dodge = _localInputReader.ReadButton(PlayerInputActionConstants.Dodge);

            input.Set(new PlayerInputData
            {
                MoveDirection = Vector2.ClampMagnitude(moveDirection, 1f),
                DodgePressed = dodge.WasPressed,
            });
        }
    }
}
