using ProjectCore.GameCore;
using ProjectCore.Template;

namespace ProjectCore.Prototype
{
    public sealed class PlayerFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindAsSingle<INetworkEntityFactory, PlayerNetworkEntityFactory>();
        }
    }
}
