namespace ProjectCore.Template
{
    public interface ILocalConfigService
    {
        TConfig GetRequiredConfig<TConfig>() where TConfig : BaseLocalConfig;

        bool TryGetConfig<TConfig>(out TConfig config) where TConfig : BaseLocalConfig;
    }
}
