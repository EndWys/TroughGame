namespace ProjectCore.GameCore
{
    public class MovementFeature
    {
        private dynamic Container { get; }

        private void Bind()
        {
            Container.BindInterfacesTo<MovementService>().AsSingle();
        }
    }
}
