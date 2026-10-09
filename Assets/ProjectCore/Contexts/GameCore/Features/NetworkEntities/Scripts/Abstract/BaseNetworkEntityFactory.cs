using System;
using Fusion;

namespace ProjectCore.GameCore
{
    public abstract class BaseNetworkEntityFactory<TPayload> : ITypedNetworkEntityFactory<TPayload>
        where TPayload : INetworkEntitySpawnPayload
    {
        public Type PayloadType => typeof(TPayload);
        public abstract NetworkEntityTypeData EntityType { get; }

        public BaseNetworkEntityRoot Spawn(
            NetworkRunner runner,
            NetworkPrefabRef prefab,
            PlayerRef? inputAuthority,
            INetworkEntitySpawnPayload payload)
        {
            if (payload is not TPayload typedPayload)
            {
                throw new ArgumentException(
                    $"Factory '{GetType().Name}' expected payload '{typeof(TPayload).Name}', " +
                    $"but received '{payload?.GetType().Name ?? "null"}'.",
                    nameof(payload));
            }

            return Spawn(runner, prefab, inputAuthority, typedPayload);
        }

        public abstract BaseNetworkEntityRoot Spawn(
            NetworkRunner runner,
            NetworkPrefabRef prefab,
            PlayerRef? inputAuthority,
            TPayload payload);

        public abstract void Despawn(BaseNetworkEntityRoot entity);
    }
}
