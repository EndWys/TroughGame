using ProjectCore.Template;

namespace ProjectCore.GameCore
{
    public sealed class MovementSimulationFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindAsSingle<IMovementCollisionStrategy, FreeMovementCollisionService>();
            BindAsSingle<IMovementSimulationService, MovementSimulationService>();
        }
    }
}
