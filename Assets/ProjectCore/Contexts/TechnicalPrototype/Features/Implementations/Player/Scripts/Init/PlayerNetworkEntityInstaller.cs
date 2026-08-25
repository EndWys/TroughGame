using ProjectCore.GameCore;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerNetworkEntityInstaller :
        BaseNetworkEntityInstaller<PlayerNetworkEntityComponent>
    {
        protected override void BindAdditionalComponents()
        {
            Container.BindInterfacesAndSelfTo<InputBufferController>()
                .AsSingle();
        }
    }
}
