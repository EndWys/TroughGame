namespace Prototype.Prototype
{
    public interface INetworkEntityComponent
    {
        public void Init();

        public void NetworkTick();

        public void ClientRender();
    }
}
