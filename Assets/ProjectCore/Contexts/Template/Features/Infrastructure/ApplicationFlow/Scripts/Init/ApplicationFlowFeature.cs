namespace ProjectCore.Template
{
    public sealed class ApplicationFlowFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindInterfacesAndSelfAsSingle<ApplicationFlowCoordinator>();
        }
    }
}
