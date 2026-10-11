using ProjectCore.Template;

namespace ProjectCore.Preloader
{
    public sealed class ApplicationInitializationFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindInterfacesAndSelfAsSingle<ApplicationInitializationFlow>();
        }
    }
}
