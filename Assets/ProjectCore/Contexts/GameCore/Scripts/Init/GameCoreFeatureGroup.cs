using ProjectCore.Template;

namespace ProjectCore.GameCore
{
    public sealed class GameCoreFeatureGroup : BaseFeatureGroup
    {
        protected override void AddFeatures()
        {
            AddFeature<NetworkEntitiesFeature>();
            AddFeature<CameraTargetFeature>();
            AddFeature<InputBufferFeature>();
            AddFeature<MovementSimulationFeature>();
            AddFeature<MovementFeature>();
            AddFeature<CombatFeature>();
        }
    }
}
