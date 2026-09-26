using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyNetworkEntityInstaller :
        BaseNetworkEntityInstaller<EnemyNetworkEntityComponent>
    {
        [SerializeField] private EnemyInputSourceComponent _inputSourceComponent;
        [SerializeField] private EnemyBehaviourStateMachine _behaviourStateMachine;
        [SerializeField] private EnemyBehaviourDisabledState _disabledState;
        [SerializeField] private EnemyBehaviourTargetSelectionState _targetSelectionState;
        [SerializeField] private EnemyBehaviourWaitingState _waitingState;

        protected override void BindAdditionalComponents()
        {
            BindComponentFromInstance(_inputSourceComponent);
            BindComponentFromInstance(_behaviourStateMachine);
            BindComponentFromInstance(_disabledState);
            BindComponentFromInstance(_targetSelectionState);
            BindComponentFromInstance(_waitingState);
            Container.Bind<EnemyTargetingService>().AsSingle();
        }
    }
}
