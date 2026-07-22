using ProjectCore.GameCore;
using ProjectCore.GameCore;
using ProjectCore.GameCore;
using ProjectCore.Template;
using ProjectCore.Prototype;
using ProjectCore.Prototype;

namespace ProjectCore.Prototype
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
