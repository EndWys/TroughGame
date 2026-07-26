using ProjectCore.Template;

namespace ProjectCore.Prototype
{
    public sealed class PrototypeFeatureGroup : BaseFeatureGroup
    {
        protected override void AddFeatures()
        {
            AddFeature<PrototypeFeature>();
            AddFeature<PlayerFeature>();
        }
    }
}
