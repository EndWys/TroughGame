using ProjectCore.Template;

namespace ProjectCore.Preloader
{
    public sealed class PreloaderContextInitializer : BaseContextInitializer, IPreloaderContextInitializer
    {
        public PreloaderContextInitializer(IFeatureInitializationFlow featureInitializationFlow)
            : base(featureInitializationFlow)
        {
        }
    }
}
