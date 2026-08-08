namespace ProjectCore.Template
{
    public sealed class LocalSaveFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindAsSingle<ILocalSavePathProvider, LocalSavePathProvider>();
            BindAsSingle<ILocalSaveSerializer, JsonLocalSaveSerializer>()
                .WhenInjectedInto<LocalSaveService>();
            BindAsSingle<ILocalSaveStorage, PlayerPrefsLocalSaveStorage>()
                .WhenInjectedInto<LocalSaveService>();
            BindAsSingle<ILocalSaveStorage, FileLocalSaveStorage>()
                .WhenInjectedInto<LocalSaveService>();
            BindAsSingle<ILocalSaveStorage, SecureFileLocalSaveStorage>()
                .WhenInjectedInto<LocalSaveService>();
            BindAsSingle<ILocalSaveService, LocalSaveService>();
        }
    }
}
