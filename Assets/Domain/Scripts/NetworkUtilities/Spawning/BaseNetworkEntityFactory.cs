using System;

namespace Domain
{
    public abstract class BaseNetworkEntityFactory<TPayload> : INetworkEntityFactory<TPayload>
        where TPayload : INetworkEntitySpawnPayload
    {
        public Type PayloadType => typeof(TPayload);
        public abstract NetworkEntityType EntityType { get; }

        public BaseNetworkEntityRoot Spawn(INetworkEntitySpawnPayload payload)
        {
            if (payload is not TPayload typedPayload)
            {
                throw new ArgumentException(
                    $"Factory '{GetType().Name}' expected payload '{typeof(TPayload).Name}', but received '{payload?.GetType().Name ?? "null"}'.",
                    nameof(payload));
            }

            return Spawn(typedPayload);
        }

        public abstract BaseNetworkEntityRoot Spawn(TPayload payload);

        public abstract void Despawn(BaseNetworkEntityRoot entity);
    }
}
