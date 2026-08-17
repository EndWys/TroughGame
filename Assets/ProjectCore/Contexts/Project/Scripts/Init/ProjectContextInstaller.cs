using ProjectCore.Template;

namespace ProjectCore.Project
{
    public sealed class ProjectContextInstaller : BaseFeatureInstaller
    {
        public override void InstallBindings()
        {
            base.InstallBindings();

            Container.BindInterfacesAndSelfTo<ProjectContextInitializer>().AsSingle();
        }

        protected override void AddFeatures()
        {
            AddFeature<LoggingFeature>();
            AddFeature<CommandLineFeature>();
            AddFeatureFromComponent<DebugToolsFeatureGroup>();
            AddFeatureFromComponent<LocalConfigFeature>();
            AddFeature<AppTimeFeature>();
            AddFeature<ObjectFactoryFeature>();
            AddFeatureFromComponent<LoadingScreenFeature>();
            AddFeature<SceneLoadingScreenBridgeFeature>();
            AddFeature<LocalSaveFeature>();
            AddFeatureFromComponent<SceneFlowFeature>();
        }
    }
}
