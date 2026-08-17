using ProjectCore.Template;

namespace ProjectCore.Preloader
{
    public sealed class PreloaderContextInstaller : BaseFeatureInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();

            Container.BindInterfacesAndSelfTo<PreloaderContextInitializer>().AsSingle();
            Container.Bind<ApplicationEntryPoint>()
                .FromComponentInHierarchy()
                .AsSingle();
        }

        protected override void AddFeatures()
        {
            AddFeature<ObjectFactoryFeature>();
            AddFeature<ApplicationInitializationFeature>();
        }
    }
}
