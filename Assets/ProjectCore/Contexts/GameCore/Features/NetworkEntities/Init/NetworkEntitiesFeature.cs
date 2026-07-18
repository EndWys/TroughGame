using Cysharp.Threading.Tasks;
using Domain;
using PhotonZenjectBridge;
using Zenject;

namespace GameCore.NetworkEntities.Init
{
    public class NetworkEntitiesFeature : IBaseFeature
    {
        private readonly DiContainer _container;

        [Inject]
        public NetworkEntitiesFeature(DiContainer container)
        {
            _container = container;
        }

        public void InstallBindings()
        {
            _container.Bind<NetworkEntityRegistry>().AsSingle();
            _container.Bind<NetworkEntityIdFactory>().AsSingle();
            _container.Bind<NetworkEntitySpawner>().AsSingle();

            _container.BindInterfacesAndSelfTo<ZenjectNetworkObjectProvider>()
                .FromComponentInHierarchy()
                .AsSingle();
        }

        public UniTask Init()
        {
            return UniTask.CompletedTask;
        }
    }
}
