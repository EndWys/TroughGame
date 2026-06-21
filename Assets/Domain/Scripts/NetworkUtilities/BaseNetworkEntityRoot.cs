using System;
using System.Collections.Generic;
using Fusion;

namespace Domain
{
    public abstract class BaseNetworkEntityRoot :
        NetworkBehaviour,
        INetworkBehaviourAccessor,
        IComposite<INetworkEntityComponent>
    {
        private readonly List<INetworkEntityComponent> _components = new();

        public IReadOnlyList<INetworkEntityComponent> Components => _components;

        public NetworkBehaviour ParentNetworkBehaviour => this;

        public override void Spawned()
        {
            BeforeComponentsInitialized();
            SetComponents(CreateComponents());
            InitComponents();
            AfterComponentsInitialized();
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
