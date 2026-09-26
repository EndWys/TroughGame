using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyBehaviourStateMachine : BaseNetworkEntityStateMachine<
        EnemyBehaviourStateType,
        EnemyBehaviourState,
        EnemyBehaviourPayload>
    {
        [SerializeField] private EnemyBehaviourDisabledState _disabledState;
        [SerializeField] private EnemyBehaviourTargetSelectionState _targetSelectionState;
        [SerializeField] private EnemyBehaviourWaitingState _waitingState;

        public override void Init()
        {
            InitializeStateMachine();

            foreach (EnemyBehaviourState state in States.Values)
            {
                state.Init();
            }

            if (ParentNetworkBehaviour.HasStateAuthority)
            {
                ChangeState(EnemyBehaviourStateType.TargetSelection);
            }
        }

        public override Dictionary<EnemyBehaviourStateType, EnemyBehaviourState>
            CreateStatesDictionary()
        {
            return new Dictionary<EnemyBehaviourStateType, EnemyBehaviourState>
            {
                { EnemyBehaviourStateType.Disabled, _disabledState },
                { EnemyBehaviourStateType.TargetSelection, _targetSelectionState },
                { EnemyBehaviourStateType.Waiting, _waitingState },
            };
        }

        public override void NetworkTick()
        {
            if (!ParentNetworkBehaviour.HasStateAuthority)
            {
                return;
            }

            UpdateStates(default);
        }
    }
}
