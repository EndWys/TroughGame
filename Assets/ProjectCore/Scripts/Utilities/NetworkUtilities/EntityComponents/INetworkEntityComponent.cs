namespace Domain
{
    public interface INetworkEntityComponent : IComposite<INetworkEntityComponent>
    {
        void Init();

        void NetworkTick();

        void ClientRender();
    }
}
