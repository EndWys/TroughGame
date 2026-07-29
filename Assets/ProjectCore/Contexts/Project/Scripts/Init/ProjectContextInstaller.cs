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
            AddFeature<AppTimeFeature>();
            AddFeature<ObjectFactoryFeature>();
            AddFeature<ApplicationFlowFeature>();
        }
    }
}
