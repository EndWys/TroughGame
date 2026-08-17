using Fusion;

namespace ProjectCore.GameCore
{
    public interface ITypedNetworkEntityFactory<in TPayload> : INetworkEntityFactory
        where TPayload : INetworkEntitySpawnPayload
    {
        BaseNetworkEntityRoot Spawn(
            NetworkRunner runner,
            NetworkPrefabRef prefab,
            PlayerRef? inputAuthority,
            TPayload payload);
    }
}
