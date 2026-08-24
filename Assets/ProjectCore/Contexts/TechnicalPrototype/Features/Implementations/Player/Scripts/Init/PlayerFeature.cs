using ProjectCore.GameCore;
using ProjectCore.Template;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class PlayerFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindAsSingle<INetworkEntityFactory, PlayerNetworkEntityFactory>();
        }
    }
}
