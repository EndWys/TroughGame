using System.Collections.Generic;

namespace Domain
{
    public static class NetworkEntityComponentTraversalExtensions
    {
        public static bool TryGetChildComponent<TComponent>(
            this IComposite<INetworkEntityComponent> composite,
            out TComponent component)
            where TComponent : class
        {
            return NetworkEntityComponentTree.TryGet(composite, out component);
        }

        public static List<TComponent> GetChildComponents<TComponent>(
            this IComposite<INetworkEntityComponent> composite)
            where TComponent : class
        {
            return NetworkEntityComponentTree.GetAll<TComponent>(composite);
        }

        public static void FillChildComponents<TComponent>(
            this IComposite<INetworkEntityComponent> composite,
            ICollection<TComponent> results)
            where TComponent : class
        {
            NetworkEntityComponentTree.Fill(composite, results);
        }

        public static bool TryGetEntityComponent<TComponent>(
            this BaseNetworkEntityRoot entity,
            out TComponent component)
            where TComponent : class
        {
            return NetworkEntityComponentTree.TryGet(entity, out component);
        }

        public static List<TComponent> GetEntityComponents<TComponent>(
            this BaseNetworkEntityRoot entity)
            where TComponent : class
        {
            return NetworkEntityComponentTree.GetAll<TComponent>(entity);
        }

        public static void FillEntityComponents<TComponent>(
            this BaseNetworkEntityRoot entity,
            ICollection<TComponent> results)
            where TComponent : class
        {
            NetworkEntityComponentTree.Fill(entity, results);
        }
    }
}
