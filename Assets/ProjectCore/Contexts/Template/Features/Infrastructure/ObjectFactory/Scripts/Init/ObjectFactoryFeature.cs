namespace ProjectCore.Template
{
    public sealed class ObjectFactoryFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindInterfacesAndSelfAsSingle<ClassFactory>();
        }
    }
}
