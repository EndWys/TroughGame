using Fusion;
using UnityEngine;
using Zenject;

namespace Domain
{
    public abstract class BaseNetworkEntityComponent : MonoBehaviour, INetworkEntityComponent
    {
        public NetworkBehaviour ParentNetworkBehaviour { get; private set; }

        [Inject]
        private void ConstructBaseNetworkEntityComponent(INetworkBehaviourAccessor networkBehaviourAccessor)
        {
            ParentNetworkBehaviour = networkBehaviourAccessor.ParentNetworkBehaviour;
        }

        public virtual void Init() { }

        public virtual void NetworkTick() { }

        public virtual void ClientRender() { }
    }
}
