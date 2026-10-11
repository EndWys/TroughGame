using ProjectCore.Template;

namespace ProjectCore.GameCore
{
    public sealed class CameraTargetFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindAsSingle<ICameraTargetRegistry, CameraTargetRegistry>();
        }
    }
}
