using ProjectCore.Template;

namespace ProjectCore.GameCore
{
    public sealed class NetworkEntitiesFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindAsSingle<NetworkEntityRegistry>();
            BindAsSingle<NetworkEntityIdFactory>();
            BindAsSingle<NetworkEntitySpawner>();
            BindAsSingle<IGameEntityComponentAccessor, GameEntityComponentAccessorService>();

            BindInterfacesAndSelfFromComponentInHierarchyAsSingle<ZenjectNetworkObjectProvider>();
        }
    }
}
