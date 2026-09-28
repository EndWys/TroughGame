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
        [SerializeField] private EnemyBehaviourChasingState _chasingState;
        [SerializeField] private EnemyMovementStateComponent _movementStateComponent;
        [SerializeField] private EnemyMovementStateMachine _movementStateMachine;
        [SerializeField] private EnemyIdleState _idleState;
        [SerializeField] private EnemyLocomotionState _locomotionState;

        protected override void BindAdditionalComponents()
        {
            BindComponentFromInstance(_inputSourceComponent);
            BindComponentFromInstance(_behaviourStateMachine);
            BindComponentFromInstance(_disabledState);
            BindComponentFromInstance(_targetSelectionState);
            BindComponentFromInstance(_waitingState);
            BindComponentFromInstance(_chasingState);
            Container.BindInterfacesAndSelfTo<EnemyMovementStateComponent>()
                .FromInstance(_movementStateComponent)
                .AsCached();
            BindComponentFromInstance(_movementStateMachine);
            BindComponentFromInstance(_idleState);
            BindComponentFromInstance(_locomotionState);
            Container.Bind<IEnemyTargetingService>().To<EnemyTargetingService>().AsSingle();
            Container.Bind<IEnemySteeringService>().To<EnemySteeringService>().AsSingle();
        }
    }
}
