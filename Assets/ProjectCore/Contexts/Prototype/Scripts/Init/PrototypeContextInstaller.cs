using ProjectCore.GameCore;
using ProjectCore.Template;

namespace ProjectCore.Prototype
{
    public sealed class PrototypeContextInstaller : BaseFeatureInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();

            Container.BindInterfacesAndSelfTo<PrototypeContextInitializer>().AsSingle();
        }

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
