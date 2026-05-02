using Fusion;
using UnityEngine;

namespace Prototype.Prototype
{
    public abstract class BaseNetworkEntityComponent : MonoBehaviour, INetworkEntityComponent
    {
        public void Init(NetworkBehaviour parentNetworkBehaviour)
        {
            Init();
        }

        public virtual void NetworkTick() { }

        public virtual void ClientRender() { }
        
        protected virtual void Init() { }
    }
}