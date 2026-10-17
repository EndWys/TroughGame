using System;
using ProjectCore.GameCore;
using Shared;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    [MovedFrom(true, "ProjectCore.TechnicalPrototype", null, "PlayerActionComponent")]
    public sealed class PlayerCombatActionComponent : BaseNetworkEntityComponent, IColleague
    {
        [SerializeField] private CombatAttackConfig _attackConfig;

        private readonly CombatActionPhaseProcessor _processor = new();
        private ICombatActionMutator _state;
        private IPlayerFacingDirectionMutator _facing;
        private IPlayerMediator _mediator;
        private CombatActionTimingData _attackTiming;

        [Inject]
        private void Construct(ICombatActionMutator state, IPlayerFacingDirectionMutator facing, IPlayerMediator mediator)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _facing = facing ?? throw new ArgumentNullException(nameof(facing));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public CombatActionSnapshotData ActionSnapshot
        {
            get
            {
                if (!IsInitialized || !HasValidNetworkContext) return default;
                CombatActionStateModel state = _state.ActionState;
                if (state.ActionId != 0 && state.ActionId != _attackConfig.ActionProfile.ActionId)
                    throw new InvalidOperationException("Unknown replicated attack ID.");
                return _processor.Evaluate(state, CombatActionKind.Attack, _attackTiming,
                    ParentNetworkBehaviour.Runner.Tick.Raw);
            }
        }

        public bool CanStartAttack => IsComponentAvailable && HasSimulationAuthority &&
            (_state.ActionState.ActionId == 0 || ActionSnapshot.IsComplete || ActionSnapshot.CanStartNextAction);

        private bool HasSimulationAuthority =>
            ParentNetworkBehaviour.HasStateAuthority || ParentNetworkBehaviour.HasInputAuthority;

        protected override void OnInitialize()
        {
            if (_attackConfig == null || _attackConfig.ActionProfile == null ||
                _attackConfig.ActionProfile.Kind != CombatActionKind.Attack)
                throw new InvalidOperationException("Player attack requires an attack definition/profile.");
            _attackTiming = CombatActionProfileValidation.CompileTiming(
                _attackConfig.ActionProfile, ParentNetworkBehaviour.Runner.DeltaTime);
            CombatShapeValidation.ValidateSettings(_attackConfig.Hitbox);
        }

        public void StartAttack(Vector2 direction)
        {
            if (!CanStartAttack) throw new InvalidOperationException("Attack cannot start in this state.");
            if (direction.sqrMagnitude <= 0f)
                direction = PlayerFacingDirectionUtility.ToVector(_facing.FacingDirection);
            CombatActionStateModel state = _state.ActionState;
            _processor.Start(ref state, _attackConfig.ActionProfile.ActionId, direction,
                ParentNetworkBehaviour.Runner.Tick.Raw);
            _state.SetActionState(state);
            _facing.SetFacingDirection(PlayerFacingDirectionUtility.FromVector(
                direction, _facing.FacingDirection, 0.01f));
            _mediator.Notify(new PlayerAttackStatePayload(this, true, true));
        }

        protected override void OnNetworkTick()
        {
            if (!HasSimulationAuthority || _state.ActionState.ActionId == 0) return;
            if (ActionSnapshot.IsComplete) StopAttack(false);
            else _mediator.Notify(new PlayerAttackStatePayload(this, true, false));
        }

        public bool TryGetAttack(ushort actionId, out CombatAttackConfig attack)
        {
            attack = IsInitialized && _attackConfig != null && _attackConfig.ActionProfile != null &&
                _attackConfig.ActionProfile.ActionId == actionId ? _attackConfig : null;
            return attack != null;
        }

        public void CancelAction()
        {
            if (IsInitialized && HasValidNetworkContext && HasSimulationAuthority) StopAttack(true);
        }

        internal void StopForTransition() { StopAttack(false); }

        private void StopAttack(bool clearInput)
        {
            CombatActionStateModel state = _state.ActionState;
            _processor.Cancel(ref state);
            _state.SetActionState(state);
            _mediator.Notify(new PlayerAttackStatePayload(this, false, false, clearInput));
        }

        protected override void OnResetLocalRuntime() { _attackTiming = default; }
    }
}
