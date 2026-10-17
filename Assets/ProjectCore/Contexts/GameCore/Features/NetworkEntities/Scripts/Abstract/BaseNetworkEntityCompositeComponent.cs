using System;
using System.Collections.Generic;

namespace ProjectCore.GameCore
{
    public abstract class BaseNetworkEntityCompositeComponent :
        BaseNetworkEntityComponent
    {
        private readonly List<INetworkEntityComponent> _components = new List<INetworkEntityComponent>();

        public override IReadOnlyList<INetworkEntityComponent> Components => _components;

        protected override void OnInitialize()
        {
            SetComponents(CreateComponents());
            InitComponents();
        }

        protected override void OnNetworkTick()
        {
            NetworkTickComponents();
        }

        protected override void OnClientRender()
        {
            ClientRenderComponents();
        }

        protected override void OnResetLocalRuntime()
        {
            try
            {
                NetworkEntityComponentLifecycleProcessor.ResetComponents(_components);
            }
            finally
            {
                _components.Clear();
            }
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
