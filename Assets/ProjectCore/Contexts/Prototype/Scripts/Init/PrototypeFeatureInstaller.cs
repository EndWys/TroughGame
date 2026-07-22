using Domain;
using GameCore.Combat.Init;
using GameCore.NetworkEntities.Init;
using GameCore.Movement.Init;
using Prototype.Player.Init;
using Prototype.Prototype.Init;

namespace Prototype.Prototype
{
    public class PrototypeFeatureInstaller : BaseFeatureInstaller
    {
        protected override void AddFeatures()
        {
            AddFeature<NetworkEntitiesFeature>();
            AddFeature<PrototypeFeature>();
            AddFeature<MovementFeature>();
            AddFeature<CombatFeature>();
            AddFeature<PlayerFeature>();
        }
    }
}
