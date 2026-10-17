using System;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerDodgeMovementProcessor :
        IMovementStateProcessor<PlayerMovementState, PlayerMovementPayload>
    {
        private readonly IMovementBodyVelocityMutator _body;
        private readonly DodgeMovementConfig _config;
        private readonly Func<DodgeMovementPhase> _phase;

        public PlayerDodgeMovementProcessor(IMovementBodyVelocityMutator body,
            DodgeMovementConfig config, Func<DodgeMovementPhase> phase)
        {
            _body = body ?? throw new ArgumentNullException(nameof(body));
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _phase = phase ?? throw new ArgumentNullException(nameof(phase));
        }

        public bool Execute(PlayerMovementPayload payload, out PlayerMovementState resultState)
        {
            DodgeMovementPhase phase = _phase();
            _body.Velocity = phase == DodgeMovementPhase.Active
                ? Vector2.ClampMagnitude(payload.Direction, 1f) * _config.Speed : Vector2.zero;
            resultState = PlayerMovementState.Idle;
            return phase == DodgeMovementPhase.None;
        }
    }
}
