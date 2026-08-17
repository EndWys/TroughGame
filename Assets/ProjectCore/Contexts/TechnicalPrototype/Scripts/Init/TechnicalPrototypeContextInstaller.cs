using ProjectCore.GameCore;
using ProjectCore.Template;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class TechnicalPrototypeContextInstaller : BaseFeatureInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();

            Container.BindInterfacesAndSelfTo<TechnicalPrototypeContextInitializer>().AsSingle();
        }

        protected override void AddFeatures()
        {
            AddFeature<ObjectFactoryFeature>();
            AddFeatureFromComponent<ScreenNavigationFeature>();
            AddFeatureFromComponent<PopupFeature>();
            AddFeature<GameCoreFeatureGroup>();
        }
    }
}
