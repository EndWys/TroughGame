using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Zenject;

namespace ProjectCore.GameCore
{
    public abstract class BaseNetworkEntityComponent : MonoBehaviour, INetworkEntityComponent
    {
        private static readonly IReadOnlyList<INetworkEntityComponent> _emptyComponents =
            Array.Empty<INetworkEntityComponent>();

        [Inject]
        private void ConstructBaseNetworkEntityComponent(INetworkBehaviourAccessor networkBehaviourAccessor)
        {
            ParentNetworkBehaviour = networkBehaviourAccessor.ParentNetworkBehaviour;
        }

        public NetworkBehaviour ParentNetworkBehaviour { get; private set; }

        public virtual IReadOnlyList<INetworkEntityComponent> Components => _emptyComponents;

        public virtual void Init() { }

        public virtual void NetworkTick() { }

        public virtual void ClientRender() { }
    }
}
