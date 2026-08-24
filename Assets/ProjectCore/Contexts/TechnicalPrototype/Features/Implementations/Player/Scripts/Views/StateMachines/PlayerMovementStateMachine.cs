using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerMovementStateMachine :
        BaseMovementStateMachine<
            PlayerMovementState,
            BaseMovementState<PlayerMovementState, PlayerMovementPayload>,
            PlayerMovementPayload>
    {
        [SerializeField] private LocomotionState _locomotionState;

        protected override PlayerMovementState InitialState =>
            PlayerMovementState.Locomotion;

        protected override Dictionary<
            PlayerMovementState,
            BaseMovementState<PlayerMovementState, PlayerMovementPayload>>
            CreateMovementStatesDictionary()
        {
            return new Dictionary<
                PlayerMovementState,
                BaseMovementState<PlayerMovementState, PlayerMovementPayload>>
            {
                { PlayerMovementState.Locomotion, _locomotionState },
            };
        }

        protected override bool TryGetMovementPayload(
            out PlayerMovementPayload payload)
        {
            if (!ParentNetworkBehaviour.GetInput(out PlayerInputData input))
            {
                payload = default;
                return false;
            }

            payload = new PlayerMovementPayload(input.MoveDirection);
            return true;
        }
    }
}
