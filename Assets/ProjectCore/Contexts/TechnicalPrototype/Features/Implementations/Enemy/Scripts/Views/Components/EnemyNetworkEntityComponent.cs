using System;
using System.Collections.Generic;
using Domain;
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
        IEnemyDashStateMutator
    {
        [SerializeField] private EnemyInputSourceComponent _inputSourceComponent;
        [SerializeField] private EnemyBehaviourStateMachine _behaviourStateMachine;
        [SerializeField] private EnemyAbilityCollectionComponent _abilityCollectionComponent;
        [SerializeField] private EnemyMovementStateComponent _movementStateComponent;
        [SerializeField] private EnemyMovementStateMachine _movementStateMachine;
        [SerializeField] private TransformMovementBodyComponent _movementBodyComponent;

        private Transform _entitiesContainer;

        [Networked] public EnemyBehaviourStateType CurrentBehaviourState { get; private set; }
        [Networked] public EnemyBehaviourStateType PreviousBehaviourState { get; private set; }
        [Networked] private NetworkString<_32> TargetEntityTypeValue { get; set; }
        [Networked] private int TargetEntityIndex { get; set; }
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
                _abilityCollectionComponent,
                _behaviourStateMachine,
                _movementStateComponent,
                _movementStateMachine,
                _movementBodyComponent,
            };
        }

        protected override void AfterComponentsInitialized()
        {
            transform.SetParent(_entitiesContainer, true);
        }

        public EnemyBehaviourStateType CurrentState => CurrentBehaviourState;

        public EnemyBehaviourStateType PreviousState => PreviousBehaviourState;

        public NetworkEntityIdData TargetEntityId
        {
            get
            {
                string entityType = TargetEntityTypeValue.ToString();

                return string.IsNullOrWhiteSpace(entityType)
                    ? NetworkEntityIdData.None
                    : new NetworkEntityIdData(new NetworkEntityTypeData(entityType), TargetEntityIndex);
            }
        }

        public Vector2 DashTargetPosition => DashStateValue.TargetPosition;

        public Vector2 DashDirection => DashStateValue.Direction;

        public TickTimer DashActionTimer => DashStateValue.ActionTimer;

        public TickTimer DashCooldownTimer => DashStateValue.CooldownTimer;

        public bool IsDashActionFinished =>
            DashActionTimer.IsRunning && DashActionTimer.ExpiredOrNotRunning(Runner);

        public bool IsDashCooldownFinished =>
            !DashCooldownTimer.IsRunning || DashCooldownTimer.ExpiredOrNotRunning(Runner);

        public bool IsStateTimerFinished => IsDashActionFinished;

        public void ChangeState(EnemyBehaviourStateType newState)
        {
            PreviousBehaviourState = CurrentBehaviourState;
            CurrentBehaviourState = newState;
        }

        public void SetTargetEntityId(NetworkEntityIdData targetEntityId)
        {
            if (!targetEntityId.IsValid)
            {
                throw new System.ArgumentException("Target entity id must be valid.", nameof(targetEntityId));
            }

            TargetEntityTypeValue = targetEntityId.Type.Value;
            TargetEntityIndex = targetEntityId.Index;
        }

        public void ClearTargetEntityId()
        {
            TargetEntityTypeValue = NetworkEntityIdData.None.Type.Value;
            TargetEntityIndex = NetworkEntityIdData.None.Index;
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
