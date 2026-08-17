using System;
using System.Collections.Generic;

namespace Domain
{
    public abstract class BaseComposite<TComponent> : IComposite<TComponent>
    {
        private readonly List<TComponent> _components = new List<TComponent>();

        public IReadOnlyList<TComponent> Components => _components;

        protected void SetComponents(IEnumerable<TComponent> components)
        {
            if (components == null)
            {
                throw new ArgumentNullException(nameof(components));
            }

            _components.Clear();

            foreach (TComponent component in components)
            {
                AddComponent(component);
            }
        }

        protected void AddComponent(TComponent component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }

            _components.Add(component);
        }

        protected bool RemoveComponent(TComponent component)
        {
            return _components.Remove(component);
        }
    }
}
