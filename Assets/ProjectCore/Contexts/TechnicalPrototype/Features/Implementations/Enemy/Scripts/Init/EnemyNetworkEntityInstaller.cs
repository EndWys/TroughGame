using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyNetworkEntityInstaller :
        BaseNetworkEntityInstaller<EnemyNetworkEntityComponent>
    {
        [SerializeField] private EnemyInputSourceComponent _inputSourceComponent;
        [SerializeField] private EnemyAbilityCollectionComponent _abilityCollectionComponent;
        [SerializeField] private EnemyMovementStateComponent _movementStateComponent;

        protected override void BindAdditionalComponents()
        {
            BindComponentFromInstance(_inputSourceComponent);
            Container.BindInterfacesAndSelfTo<EnemyAbilityCollectionComponent>()
                .FromInstance(_abilityCollectionComponent)
                .AsCached();
            Container.BindInterfacesAndSelfTo<EnemyMovementStateComponent>()
                .FromInstance(_movementStateComponent)
                .AsCached();

            Container.Bind<IEnemyTargetingService>().To<EnemyTargetingService>().AsSingle();
            Container.Bind<IEnemySteeringService>().To<EnemySteeringService>().AsSingle();
        }
    }
}
