using System;
using System.Collections.Generic;
using Shared;
using Fusion;
using Zenject;

namespace ProjectCore.GameCore
{
    public abstract class BaseNetworkEntityRoot :
        NetworkBehaviour,
        INetworkBehaviourAccessor,
        INetworkEntityIdAccessor,
        IEntityPositionAccessor,
        IComposite<INetworkEntityComponent>
    {
        private readonly List<INetworkEntityComponent> _components = new();
        private readonly NetworkEntityComponentLifecycleProcessor _lifecycle = new();
        private NetworkEntityRegistry _networkEntityRegistry;
        private NetworkEntityIdData _registeredEntityId;

        [Networked] private uint NetworkEntityIdValue { get; set; }

        [Inject]
        private void ConstructBaseNetworkEntityRoot(
            [InjectOptional] NetworkEntityRegistry networkEntityRegistry)
        {
            _networkEntityRegistry = networkEntityRegistry;
        }

        public IReadOnlyList<INetworkEntityComponent> Components => _components;

        public NetworkBehaviour ParentNetworkBehaviour => this;

        public UnityEngine.Vector2 Position => transform.position;

        public NetworkEntityIdData EntityId => new(NetworkEntityIdValue);

        public void SetEntityId(NetworkEntityIdData entityId)
        {
            NetworkEntityIdValidation.ValidateAssignedId(entityId, nameof(entityId));
            NetworkEntityIdValue = entityId.Value;
        }

        public bool TryGetRootContract<TContract>(out TContract contract)
            where TContract : class
        {
            contract = this as TContract;
            return contract != null;
        }

        public override void Spawned()
        {
            _lifecycle.Initialize(InitializeRuntime, ResetLocalRuntime);
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            _lifecycle.Reset(ResetLocalRuntime);
        }

        public override void FixedUpdateNetwork()
        {
            if (_lifecycle.IsInitialized) NetworkTickComponents();
        }

        public override void Render()
        {
            if (_lifecycle.IsInitialized) ClientRenderComponents();
        }

        protected abstract IEnumerable<INetworkEntityComponent> CreateComponents();

        protected virtual void BeforeComponentsInitialized() { }

        protected virtual void AfterComponentsInitialized() { }

        private void InitializeRuntime()
        {
            BeforeComponentsInitialized();
            RegisterEntity();
            SetComponents(CreateComponents());
            InitComponents();
            AfterComponentsInitialized();
        }

        private void ResetLocalRuntime()
        {
            try
            {
                NetworkEntityComponentLifecycleProcessor.ResetComponents(_components);
            }
            finally
            {
                _components.Clear();
                UnregisterEntity();
            }
        }

        private void RegisterEntity()
        {
            if (_networkEntityRegistry == null || !EntityId.IsValid)
            {
                return;
            }

            _networkEntityRegistry.Register(this);
            _registeredEntityId = EntityId;
        }

        private void UnregisterEntity()
        {
            _networkEntityRegistry?.Unregister(this, _registeredEntityId);
            _registeredEntityId = NetworkEntityIdData.None;
        }

        private void SetComponents(IEnumerable<INetworkEntityComponent> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            _components.Clear();

            foreach (INetworkEntityComponent component in components)
            {
                AddComponent(component);
            }
        }

        private void AddComponent(INetworkEntityComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            _components.Add(component);
        }

        private void InitComponents()
        {
            foreach (INetworkEntityComponent component in _components)
            {
                component.Init();
            }
        }

        private void NetworkTickComponents()
        {
            foreach (INetworkEntityComponent component in _components)
            {
                component.NetworkTick();
            }
        }

        private void ClientRenderComponents()
        {
            foreach (INetworkEntityComponent component in _components)
            {
                component.ClientRender();
            }
        }
    }
}
