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
            LocalButtonStateData attack = _localInputReader.ReadButton(PlayerInputActionConstants.Attack);
            bool dodgeEligible = dodge.WasPressed && moveDirection.sqrMagnitude > 0f;
            ushort commandId = attack.WasPressed && (!dodgeEligible || attack.PressSequence > dodge.PressSequence)
                ? PlayerInputActionConstants.AttackCommandId
                : dodgeEligible ? MovementInputCommandConstants.DodgeCommandId : (ushort)0;

            input.Set(new PlayerInputData
            {
                MoveDirection = Vector2.ClampMagnitude(moveDirection, 1f),
                DodgePressed = dodge.WasPressed,
                LatestActionCommandId = commandId,
            });
        }
    }
}
