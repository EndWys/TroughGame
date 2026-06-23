using System;
using System.Collections.Generic;

namespace Domain
{
    public static class NetworkEntityComponentTree
    {
        public static bool TryGet<TComponent>(
            IComposite<INetworkEntityComponent> composite,
            out TComponent result)
            where TComponent : class
        {
            if (composite == null)
            {
                throw new ArgumentNullException(nameof(composite));
            }

            foreach (INetworkEntityComponent component in composite.Components)
            {
                if (component is TComponent typedComponent)
                {
                    result = typedComponent;
                    return true;
                }

                if (TryGet(component, out result))
                {
                    return true;
                }
            }

            result = null;
            return false;
        }

        public static List<TComponent> GetAll<TComponent>(IComposite<INetworkEntityComponent> composite)
            where TComponent : class
        {
            var results = new List<TComponent>();
            Fill(composite, results);

            return results;
        }

        public static void Fill<TComponent>(
            IComposite<INetworkEntityComponent> composite,
            ICollection<TComponent> results)
            where TComponent : class
        {
            if (composite == null)
            {
                throw new ArgumentNullException(nameof(composite));
            }

            if (results == null)
            {
                throw new ArgumentNullException(nameof(results));
            }

            foreach (INetworkEntityComponent component in composite.Components)
            {
                if (component is TComponent typedComponent)
                {
                    results.Add(typedComponent);
                }

                Fill(component, results);
            }
        }
    }
}
