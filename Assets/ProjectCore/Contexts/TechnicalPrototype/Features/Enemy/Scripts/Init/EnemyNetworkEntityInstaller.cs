using ProjectCore.Template;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyNetworkEntityInstaller :
        BaseNetworkEntityInstaller<EnemyNetworkEntityComponent>
    {
        [SerializeField] private EnemyInputSourceComponent _inputSourceComponent;
        [SerializeField] private EnemyAbilityCollectionComponent _abilityCollectionComponent;

        [SerializeField] private TransformMovementBodyComponent _movementBody;

        protected override void BindAdditionalComponents()
        {
            Container.BindInterfacesAndSelfTo<ClassFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<TransformMovementBodyComponent>().FromInstance(_movementBody).AsSingle();
            BindComponentFromInstance(_inputSourceComponent);
            Container.BindInterfacesAndSelfTo<EnemyAbilityCollectionComponent>()
                .FromInstance(_abilityCollectionComponent)
                .AsCached();

            Container.Bind<IEnemyTargetingService>().To<EnemyTargetingService>().AsSingle();
            Container.Bind<IEnemySteeringService>().To<EnemySteeringService>().AsSingle();
        }
    }
}
