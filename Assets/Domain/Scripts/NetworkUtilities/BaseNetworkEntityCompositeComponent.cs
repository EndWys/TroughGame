using System;
using System.Collections.Generic;

namespace Domain
{
    public abstract class BaseNetworkEntityCompositeComponent :
        BaseNetworkEntityComponent,
        IComposite<INetworkEntityComponent>
    {
        private readonly List<INetworkEntityComponent> _components = new List<INetworkEntityComponent>();

        public IReadOnlyList<INetworkEntityComponent> Components => _components;

        public override void Init()
        {
            SetComponents(CreateComponents());
            InitComponents();
        }

        public override void NetworkTick()
        {
            NetworkTickComponents();
        }

        public override void ClientRender()
        {
            ClientRenderComponents();
        }

        protected abstract IEnumerable<INetworkEntityComponent> CreateComponents();

        protected void SetComponents(IEnumerable<INetworkEntityComponent> components)
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

        protected void AddComponent(INetworkEntityComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            _components.Add(component);
        }

        protected void InitComponents()
        {
            foreach (INetworkEntityComponent component in _components)
            {
                component.Init();
            }
        }

        protected void NetworkTickComponents()
        {
            foreach (INetworkEntityComponent component in _components)
            {
                component.NetworkTick();
            }
        }

        protected void ClientRenderComponents()
        {
            foreach (INetworkEntityComponent component in _components)
            {
                component.ClientRender();
            }
        }
    }
}
