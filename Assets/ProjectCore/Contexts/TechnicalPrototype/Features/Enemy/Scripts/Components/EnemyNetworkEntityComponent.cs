using System;
using System.Collections.Generic;
using Shared;
using Fusion;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyNetworkEntityComponent :
        BaseNetworkEntityRoot,
        IEnemyBehaviourStateMutator,
        IEnemyTargetMutator,
        IEnemyDashStateMutator,
        IStateDataMutator<EnemyEntityState>
    {
        [SerializeField] private EnemyInputSourceComponent _inputSourceComponent;
        [SerializeField] private CombatHurtboxComponent _combatHurtboxComponent;
        [SerializeField] private CombatDebugVisualizationComponent _combatDebugVisualizationComponent;
        [SerializeField] private EnemyBehaviourStateMachine _behaviourStateMachine;
        [SerializeField] private EnemyAbilityCollectionComponent _abilityCollectionComponent;
        [SerializeField, UnityEngine.Serialization.FormerlySerializedAs("_movementStateMachine")] private EnemyEntityStateMachine _entityStateMachine;
        [SerializeField] private TransformMovementBodyComponent _movementBodyComponent;

        private Transform _entitiesContainer;

        [Networked] public EnemyEntityState CurrentEntityState { get; private set; }
        [Networked] public EnemyEntityState PreviousEntityState { get; private set; }

        [Networked] public EnemyBehaviourStateType CurrentBehaviourState { get; private set; }
        [Networked] public EnemyBehaviourStateType PreviousBehaviourState { get; private set; }
        [Networked] private uint TargetEntityIdValue { get; set; }
        [Networked] private EnemyDashStateModel DashStateValue { get; set; }

        [Inject]
        private void Construct(
            [Inject(Id = EnemyNetworkEntityConstants.EntitiesContainer)] Transform entitiesContainer)
        {
            _entitiesContainer = entitiesContainer != null
                ? entitiesContainer
                : throw new ArgumentNullException(nameof(entitiesContainer));
        }

        protected override IEnumerable<INetworkEntityComponent> CreateComponents()
        {
            return new INetworkEntityComponent[]
            {
                _inputSourceComponent,
                _combatHurtboxComponent,
                _abilityCollectionComponent,
                _behaviourStateMachine,
                _entityStateMachine,
                _movementBodyComponent,
                _combatDebugVisualizationComponent,
            };
        }

        protected override void AfterComponentsInitialized()
        {
            transform.SetParent(_entitiesContainer, true);
        }


        public EnemyBehaviourStateType CurrentState => CurrentBehaviourState;

        public EnemyBehaviourStateType PreviousState => PreviousBehaviourState;

        EnemyEntityState IStateDataAccessor<EnemyEntityState>.CurrentState => CurrentEntityState;
        EnemyEntityState IStateDataAccessor<EnemyEntityState>.PreviousState => PreviousEntityState;

        void IStateDataMutator<EnemyEntityState>.ChangeState(EnemyEntityState state)
        {
            PreviousEntityState = CurrentEntityState;
            CurrentEntityState = state;
        }

        public NetworkEntityIdData TargetEntityId => new(TargetEntityIdValue);

        public Vector2 DashTargetPosition => DashStateValue.TargetPosition;

        public Vector2 DashDirection => DashStateValue.Direction;

        public TickTimer DashActionTimer => DashStateValue.ActionTimer;

        public TickTimer DashCooldownTimer => DashStateValue.CooldownTimer;

        public bool IsDashActionFinished =>
            DashActionTimer.IsRunning && DashActionTimer.ExpiredOrNotRunning(Runner);

        public bool IsDashCooldownFinished =>
            !DashCooldownTimer.IsRunning || DashCooldownTimer.ExpiredOrNotRunning(Runner);

        public int? StateTimerEndTick => DashActionTimer.IsRunning ? DashActionTimer.TargetTick : null;

        public bool IsStateTimerFinished => IsDashActionFinished;

        public void ChangeState(EnemyBehaviourStateType newState)
        {
            PreviousBehaviourState = CurrentBehaviourState;
            CurrentBehaviourState = newState;
        }

        public void SetTargetEntityId(NetworkEntityIdData targetEntityId)
        {
            NetworkEntityIdValidation.ValidateAssignedId(targetEntityId, nameof(targetEntityId));
            TargetEntityIdValue = targetEntityId.Value;
        }

        public void ClearTargetEntityId()
        {
            TargetEntityIdValue = NetworkEntityIdData.None.Value;
        }

        public void StartDash(
            Vector2 targetPosition,
            Vector2 direction,
            float durationSeconds,
            float cooldownSeconds)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
            {
                throw new ArgumentException("Dash direction must be non-zero.", nameof(direction));
            }

            if (durationSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(durationSeconds));
            }

            if (cooldownSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(cooldownSeconds));
            }

            DashStateValue = new EnemyDashStateModel
            {
                TargetPosition = targetPosition,
                Direction = direction.normalized,
                ActionTimer = TickTimer.CreateFromSeconds(Runner, durationSeconds),
                CooldownTimer = TickTimer.CreateFromSeconds(Runner, cooldownSeconds),
            };
        }

        public void StopDash()
        {
            EnemyDashStateModel state = DashStateValue;
            state.ActionTimer = TickTimer.None;
            DashStateValue = state;
        }
    }
}
