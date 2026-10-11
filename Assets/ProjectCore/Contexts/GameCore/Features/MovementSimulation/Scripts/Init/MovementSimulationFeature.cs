using ProjectCore.Template;

namespace ProjectCore.GameCore
{
    public sealed class MovementSimulationFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindInterfacesAsSingle<LevelCollisionService>();
            BindAsSingle<IMovementSimulationService, MovementSimulationService>();
        }
    }
}
