using Cysharp.Threading.Tasks;
using System.Threading;
using ProjectCore.Template;
using Zenject;

namespace ProjectCore.GameCore
{
    public sealed class NetworkEntitiesFeature : IBaseFeature
    {
        public void InstallBindings(DiContainer container)
        {
            container.Bind<NetworkEntityRegistry>().AsSingle();
            container.Bind<NetworkEntityIdFactory>().AsSingle();
            container.Bind<NetworkEntitySpawner>().AsSingle();
            container.Bind<IGameEntityComponentAccessor>()
                .To<GameEntityComponentAccessorService>()
                .AsSingle();

            container.BindInterfacesAndSelfTo<ZenjectNetworkObjectProvider>()
                .FromComponentInHierarchy()
                .AsSingle();
        }

        public UniTask InitializeAsync(
            DiContainer container,
            CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
