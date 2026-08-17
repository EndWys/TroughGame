using ProjectCore.Template;

namespace ProjectCore.Project
{
    public sealed class ProjectContextInitializer : BaseContextInitializer, IProjectContextInitializer
    {
        public ProjectContextInitializer(IFeatureInitializationFlow featureInitializationFlow)
            : base(featureInitializationFlow)
        {
        }
    }
}
