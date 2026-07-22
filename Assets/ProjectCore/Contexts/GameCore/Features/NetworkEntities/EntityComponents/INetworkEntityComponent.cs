using Domain;

namespace ProjectCore.GameCore
{
    public interface INetworkEntityComponent : IComposite<INetworkEntityComponent>
    {
        void Init();

        void NetworkTick();

        void ClientRender();
    }
}
