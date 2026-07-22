using System;
using System.Collections.Generic;
using Fusion;
using UnityEngine;
using Zenject;

namespace ProjectCore.GameCore
{
    public abstract class BaseNetworkEntityComponent : MonoBehaviour, INetworkEntityComponent
    {
        private static readonly IReadOnlyList<INetworkEntityComponent> EmptyComponents =
            Array.Empty<INetworkEntityComponent>();

        public NetworkBehaviour ParentNetworkBehaviour { get; private set; }

        public virtual IReadOnlyList<INetworkEntityComponent> Components => EmptyComponents;

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
