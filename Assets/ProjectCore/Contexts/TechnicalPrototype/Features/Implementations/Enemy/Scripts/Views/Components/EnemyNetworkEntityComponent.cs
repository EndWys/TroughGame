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
        IEnemyTargetMutator
    {
        [SerializeField] private EnemyInputSourceComponent _inputSourceComponent;
        [SerializeField] private EnemyBehaviourStateMachine _behaviourStateMachine;

        private Transform _entitiesContainer;

        [Networked] public EnemyBehaviourStateType CurrentBehaviourState { get; private set; }
        [Networked] public EnemyBehaviourStateType PreviousBehaviourState { get; private set; }
        [Networked] private NetworkString<_32> TargetEntityTypeValue { get; set; }
        [Networked] private int TargetEntityIndex { get; set; }

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
                _behaviourStateMachine,
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
    }
}
