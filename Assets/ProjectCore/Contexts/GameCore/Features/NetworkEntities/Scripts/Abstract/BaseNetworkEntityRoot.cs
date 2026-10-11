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
        IComposite<INetworkEntityComponent>
    {
        private readonly List<INetworkEntityComponent> _components = new();
        private NetworkEntityRegistry _networkEntityRegistry;

        [Networked] private uint NetworkEntityIdValue { get; set; }

        [Inject]
        private void ConstructBaseNetworkEntityRoot(
            [InjectOptional] NetworkEntityRegistry networkEntityRegistry)
        {
            _networkEntityRegistry = networkEntityRegistry;
        }

        public IReadOnlyList<INetworkEntityComponent> Components => _components;

        public NetworkBehaviour ParentNetworkBehaviour => this;

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
            BeforeComponentsInitialized();
            RegisterEntity();
            SetComponents(CreateComponents());
            InitComponents();
            AfterComponentsInitialized();
        }

        public override void Despawned(NetworkRunner runner, bool hasState)
        {
            UnregisterEntity();
        }

        public override void FixedUpdateNetwork()
        {
            NetworkTickComponents();
        }

        public override void Render()
        {
            ClientRenderComponents();
        }

        protected abstract IEnumerable<INetworkEntityComponent> CreateComponents();

        protected virtual void BeforeComponentsInitialized() { }

        protected virtual void AfterComponentsInitialized() { }

        private void RegisterEntity()
        {
            if (_networkEntityRegistry == null || !EntityId.IsValid)
            {
                return;
            }

            _networkEntityRegistry.Register(this);
        }

        private void UnregisterEntity()
        {
            _networkEntityRegistry?.Unregister(this);
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
