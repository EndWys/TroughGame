using Fusion;

namespace Prototype.Prototype
{
    public interface INetworkEntityComponent
    {
        public void Init(NetworkBehaviour parentNetworkBehaviour);
        
        public void NetworkTick();

        public void ClientRender();
    }
}