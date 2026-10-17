using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using Shared;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    [UnityEngine.Scripting.APIUpdating.MovedFrom(true, sourceNamespace: "ProjectCore.TechnicalPrototype", sourceAssembly: "ProjectCore.Runtime", sourceClassName: "DodgeState")]
    public sealed class PlayerDodgeState : BaseMovementState<PlayerMovementState, PlayerMovementPayload>, IColleague
    {
        [SerializeField] private TransformMovementBodyComponent _movementBody;
        [SerializeField] private DodgeMovementConfig _dodgeMovementConfig;
        private IMovementStateTimerMutator _timer;
        private INetworkBehaviourAccessor _network;
        private IPlayerMediator _mediator;
        private DodgeTimingData? _timing;

        [Inject]
        private void Construct(IMovementStateTimerMutator timer, INetworkBehaviourAccessor network, IPlayerMediator mediator)
        {
            _timer = timer ?? throw new ArgumentNullException(nameof(timer));
            _network = network ?? throw new ArgumentNullException(nameof(network));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        private DodgeTimingData Timing => _timing ??= MovementConfigValidation.CompileDodgeTiming(
            _dodgeMovementConfig, _network.ParentNetworkBehaviour.Runner.DeltaTime);

        public DodgeMovementPhase Phase => DodgePhaseUtility.Evaluate(
            Timing, _timer.StateTimerEndTick, _network.ParentNetworkBehaviour.Runner.Tick.Raw);

        public int? ActiveEndTick => _timer.StateTimerEndTick.HasValue
            ? unchecked(_timer.StateTimerEndTick.Value - Timing.RecoveryTicks) : null;

        public override void Init()
        {
            _timing = null;
            base.Init();
        }

        public override void Enter()
        {
            ((IMovementBodyVelocityMutator)_movementBody).Velocity = Vector2.zero;
            // A restored network timer already identifies the execution; never restart it on correction.
            if (!_timer.StateTimerEndTick.HasValue) _timer.StartStateTimerTicks(Timing.TotalTicks);
        }

        public override PlayerMovementState Tick(PlayerMovementPayload payload)
        {
            PlayerMovementState next = base.Tick(payload);
            _mediator.Notify(new PlayerDodgePhasePayload(this, Phase, ActiveEndTick));
            return next;
        }

        public override void Exit()
        {
            ((IMovementBodyVelocityMutator)_movementBody).Velocity = Vector2.zero;
            _timer.StopStateTimer();
            _mediator.Notify(new PlayerDodgePhasePayload(this, DodgeMovementPhase.None, null));
        }

        protected override IReadOnlyList<IMovementStateProcessor<PlayerMovementState, PlayerMovementPayload>>
            CreateMovementProcessors()
        {
            if (_movementBody == null || _dodgeMovementConfig == null)
                throw new InvalidOperationException("Dodge requires a movement body and config.");
            MovementConfigValidation.ValidateDodge(_dodgeMovementConfig);
            return new IMovementStateProcessor<PlayerMovementState, PlayerMovementPayload>[]
            {
                new PlayerDodgeMovementProcessor(_movementBody, _dodgeMovementConfig, () => Phase),
            };
        }

        protected override PlayerMovementState FallbackState => PlayerMovementState.Dodge;
    }
}
