using System;
using System.Collections.Generic;
using Fusion;
using Zenject;

namespace Domain
{
    public abstract class BaseNetworkEntityRoot :
        NetworkBehaviour,
        INetworkBehaviourAccessor,
        IComposite<INetworkEntityComponent>
    {
        private readonly List<INetworkEntityComponent> _components = new();
        private NetworkEntityRegistry _networkEntityRegistry;

        [Networked] private NetworkString<_32> NetworkEntityTypeValue { get; set; }
        [Networked] private int NetworkEntityIndex { get; set; }

        [Inject]
        private void ConstructBaseNetworkEntityRoot([InjectOptional] NetworkEntityRegistry networkEntityRegistry)
        {
            _networkEntityRegistry = networkEntityRegistry;
        }

        public IReadOnlyList<INetworkEntityComponent> Components => _components;

        public NetworkBehaviour ParentNetworkBehaviour => this;

        public NetworkEntityId EntityId
        {
            get
            {
                string entityType = NetworkEntityTypeValue.ToString();

                return string.IsNullOrWhiteSpace(entityType)
                    ? NetworkEntityId.None
                    : new NetworkEntityId(new NetworkEntityType(entityType), NetworkEntityIndex);
            }
        }

        public void SetEntityId(NetworkEntityId entityId)
        {
            if (!entityId.IsValid)
            {
                throw new ArgumentException("Network entity id must be valid.", nameof(entityId));
            }

            NetworkEntityTypeValue = entityId.Type.Value;
            NetworkEntityIndex = entityId.Index;
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
