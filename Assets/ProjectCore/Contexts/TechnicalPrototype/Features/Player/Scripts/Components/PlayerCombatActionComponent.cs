using System;
using ProjectCore.GameCore;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    [MovedFrom(true, "ProjectCore.TechnicalPrototype", null, "PlayerActionComponent")]
    public sealed class PlayerCombatActionComponent : BaseNetworkEntityComponent
    {
        [SerializeField] private CombatAttackConfig _attackConfig;

        private CombatActionPhaseProcessor _processor;
        private ICombatActionMutator _state;
        private ActionTimingData _attackTiming;

        [Inject]
        private void Construct(ICombatActionMutator state, ProjectCore.Template.IClassFactory factory)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
            _processor = factory.Create<CombatActionPhaseProcessor>();
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
            if (!IsComponentAvailable || !HasSimulationAuthority) throw new InvalidOperationException("Attack cannot start in this state.");
            if (direction.sqrMagnitude <= 0f)
                throw new ArgumentException("Attack direction must be nonzero.", nameof(direction));
            CombatActionStateModel state = _state.ActionState;
            _processor.Start(ref state, _attackConfig.ActionProfile.ActionId, direction,
                ParentNetworkBehaviour.Runner.Tick.Raw);
            _state.SetActionState(state);
        }

        public bool TryGetAttack(ushort actionId, out CombatAttackConfig attack)
        {
            attack = IsInitialized && _attackConfig != null && _attackConfig.ActionProfile != null &&
                _attackConfig.ActionProfile.ActionId == actionId ? _attackConfig : null;
            return attack != null;
        }

        internal void StopForTransition() { StopAttack(); }

        private void StopAttack()
        {
            CombatActionStateModel state = _state.ActionState;
            _processor.Cancel(ref state);
            _state.SetActionState(state);
        }

        protected override void OnResetLocalRuntime() { _attackTiming = default; }
    }
}
