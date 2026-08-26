using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerNetworkEntityInstaller :
        BaseNetworkEntityInstaller<PlayerNetworkEntityComponent>
    {
        [SerializeField] private PlayerMediatorComponent _playerMediatorComponent;

        protected override void BindAdditionalComponents()
        {
            Container.BindInterfacesAndSelfTo<PlayerMediatorComponent>()
                .FromInstance(_playerMediatorComponent)
                .AsCached();

            Container.BindInterfacesAndSelfTo<InputBufferController>()
                .AsSingle();
        }
    }
}
