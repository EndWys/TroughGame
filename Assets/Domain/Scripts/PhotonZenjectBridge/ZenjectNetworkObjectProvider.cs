using Fusion;
using Zenject;

namespace PhotonZenjectBridge
{
    public class ZenjectNetworkObjectProvider : NetworkObjectProviderDefault
    {
        [Inject] private DiContainer _container;

        protected override NetworkObject InstantiatePrefab(NetworkRunner runner, NetworkObject prefab)
        {
            var networkObject = base.InstantiatePrefab(runner, prefab);

            if (networkObject != null)
            {
                _container.InjectGameObject(networkObject.gameObject);
            }

            return networkObject;
        }
    }
}