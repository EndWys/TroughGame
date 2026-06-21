namespace Domain
{
    public interface INetworkEntityComponent
    {
        void Init();

        void NetworkTick();

        void ClientRender();
    }
}
