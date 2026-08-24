using ProjectCore.Template;

namespace ProjectCore.GameCore
{
    public sealed class InputBufferFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindAsSingle<IInputBufferSystem, InputBufferSystem>();
        }
    }
}
