using System;
using System.Collections.Generic;
using Fusion;
using Zenject;

namespace ProjectCore.GameCore
{
    public abstract class BaseNetworkEntityBehaviourComponent : NetworkBehaviour, INetworkEntityComponent
    {
        private static readonly IReadOnlyList<INetworkEntityComponent> _emptyComponents =
            Array.Empty<INetworkEntityComponent>();

        private readonly NetworkEntityComponentLifecycleProcessor _lifecycle = new();
        private INetworkEntityIdAccessor _entityIdAccessor;

        [Inject]
        private void ConstructBaseNetworkEntityBehaviourComponent(
            INetworkBehaviourAccessor networkBehaviourAccessor,
            INetworkEntityIdAccessor entityIdAccessor)
        {
            ParentNetworkBehaviour = networkBehaviourAccessor.ParentNetworkBehaviour;
            _entityIdAccessor = entityIdAccessor ?? throw new ArgumentNullException(nameof(entityIdAccessor));
        }

        public NetworkBehaviour ParentNetworkBehaviour { get; private set; }

        public bool IsInitialized => _lifecycle.IsInitialized;

        public bool HasValidNetworkContext => _lifecycle.HasValidNetworkContext(ParentNetworkBehaviour);

        public bool HasSimulationAuthority =>
            HasValidNetworkContext
            && (ParentNetworkBehaviour.HasStateAuthority || ParentNetworkBehaviour.HasInputAuthority);

        public bool IsComponentAvailable =>
            IsInitialized && HasValidNetworkContext && isActiveAndEnabled && IsComponentValid;

        public virtual IReadOnlyList<INetworkEntityComponent> Components => _emptyComponents;

        protected NetworkEntityIdData EntityId => _entityIdAccessor.EntityId;

        protected virtual bool IsComponentValid => true;

        public void Init()
        {
            _lifecycle.Initialize(OnInitialize, OnResetLocalRuntime);
        }

        public void NetworkTick()
        {
            if (!TryBeginCallback())
                return;

            OnNetworkTick();
        }

        public void ClientRender()
        {
            if (!TryBeginCallback())
                return;

            OnClientRender();
        }

        public void ResetLocalRuntime()
        {
            _lifecycle.Reset(OnResetLocalRuntime);
        }

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnNetworkTick()
        {
        }

        protected virtual void OnClientRender()
        {
        }

        protected virtual void OnResetLocalRuntime()
        {
        }

        protected virtual void OnNetworkContextInvalidated()
        {
        }

        private bool TryBeginCallback()
        {
            if (IsComponentAvailable)
                return true;

            if (IsInitialized && !HasValidNetworkContext)
                OnNetworkContextInvalidated();

            return false;
        }
    }
}
