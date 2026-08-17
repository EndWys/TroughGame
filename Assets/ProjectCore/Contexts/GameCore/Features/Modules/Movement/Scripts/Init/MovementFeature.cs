using ProjectCore.Template;

namespace ProjectCore.GameCore
{
    public sealed class MovementFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindAsSingle<IMovementSystem, MovementSystem>();
        }
    }
}
