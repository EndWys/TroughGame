using System;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerDodgeMovementProcessor :
        IMovementStateProcessor<PlayerEntityState, PlayerEntityPayload>
    {
        private readonly IMovementBodyVelocityMutator _body;
        private readonly DodgeMovementConfig _config;
        private readonly Func<ActionPhase> _phase;

        public PlayerDodgeMovementProcessor(
            IMovementBodyVelocityMutator body,
            DodgeMovementConfig config,
            Func<ActionPhase> phase)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _phase = phase ?? throw new ArgumentNullException(nameof(phase));
        }

        public bool Execute(PlayerEntityPayload payload, out PlayerEntityState resultState)
        {
            ActionPhase phase = _phase();

            _body.Velocity = phase == ActionPhase.Active
                ?
                Vector2.ClampMagnitude(payload.Input.MoveDirection, 1f) * _config.Speed
                :
                Vector2.zero;

            resultState = PlayerEntityState.Idle;
            return phase == ActionPhase.None;
        }
    }
}
