using ProjectCore.Template;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerNetworkEntityInstaller :
        BaseNetworkEntityInstaller<PlayerNetworkEntityComponent>
    {
        [SerializeField] private PlayerMediatorComponent _playerMediatorComponent;

        [SerializeField] private TransformMovementBodyComponent _movementBody;
        [SerializeField] private PlayerCombatActionComponent _combatAction;
        [SerializeField] private PlayerEntityStateMachine _entityStateMachine;
        [SerializeField] private PlayerInputSourceComponent _inputSource;

        protected override void BindAdditionalComponents()
        {
            Container.BindInterfacesAndSelfTo<ClassFactory>().AsSingle();
            Container.BindInterfacesAndSelfTo<TransformMovementBodyComponent>().FromInstance(_movementBody).AsSingle();
            BindComponentFromInstance(_combatAction);
            Container.Bind<IEntityStateRestart>().FromInstance(_entityStateMachine).AsSingle();
            Container.Bind<IPlayerInputAccessor>().FromInstance(_inputSource).AsSingle();
            Container.BindInterfacesAndSelfTo<PlayerMediatorComponent>()
                .FromInstance(_playerMediatorComponent)
                .AsCached();

            Container.BindInterfacesAndSelfTo<InputBufferController>()
                .AsSingle();
        }
    }
}
