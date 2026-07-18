using Domain;
using Prototype.Movement.Init;
using Prototype.Player.Init;
using Prototype.Prototype.Init;

namespace Prototype.Prototype
{
    public class PrototypeFeatureInstaller : BaseFeatureInstaller
    {
        protected override void AddFeatures()
        {
            AddFeature<PrototypeFeature>();
            AddFeature<MovementFeature>();
            AddFeature<PlayerFeature>();
        }
    }
}
