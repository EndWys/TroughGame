using Domain;
using GameCore.NetworkEntites.Init;
using GameCore.Movement.Init;
using Prototype.Player.Init;
using Prototype.Prototype.Init;

namespace Prototype.Prototype
{
    public class PrototypeFeatureInstaller : BaseFeatureInstaller
    {
        protected override void AddFeatures()
        {
            AddFeature<NetworkEntitesFeature>();
            AddFeature<PrototypeFeature>();
            AddFeature<MovementFeature>();
            AddFeature<PlayerFeature>();
        }
    }
}
