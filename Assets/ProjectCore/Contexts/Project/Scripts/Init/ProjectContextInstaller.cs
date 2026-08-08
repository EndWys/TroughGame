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
            AddFeatureFromComponent<LocalConfigFeature>();
            AddFeature<AppTimeFeature>();
            AddFeature<ObjectFactoryFeature>();
            AddFeature<LocalSaveFeature>();
            AddFeatureFromComponent<SceneFlowFeature>();
        }
    }
}
