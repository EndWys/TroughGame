using System;
using System.Collections.Generic;
using Zenject;

namespace ProjectCore.Template
{
    public sealed class LocalConfigService : ILocalConfigService
    {
        private readonly Dictionary<Type, BaseLocalConfig> _configs =
            new Dictionary<Type, BaseLocalConfig>();
        private readonly ILocalConfigService _parentService;
        private LocalConfigCatalogConfig _catalog;
        private bool _isInitialized;

        public LocalConfigService(
            [Inject(Source = InjectSources.AnyParent, Optional = true)]
            ILocalConfigService parentService)
        {
            _parentService = parentService;
        }

        public void Initialize(LocalConfigCatalogConfig catalog)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("LocalConfigService has already been initialized.");
            }

            LocalConfigCatalogValidation.Validate(catalog);
            _catalog = catalog;

            IReadOnlyList<BaseLocalConfig> configs = _catalog.Configs;
            for (int i = 0; i < configs.Count; i++)
            {
                BaseLocalConfig config = configs[i];
                _configs.Add(config.GetType(), config);
            }

            _isInitialized = true;
        }

        public TConfig GetRequiredConfig<TConfig>() where TConfig : BaseLocalConfig
        {
            if (TryGetConfig(out TConfig config))
            {
                return config;
            }

            throw new InvalidOperationException(
                $"Local config '{typeof(TConfig).Name}' was not found in the current or parent contexts.");
        }

        public bool TryGetConfig<TConfig>(out TConfig config) where TConfig : BaseLocalConfig
        {
            EnsureInitialized();

            if (_configs.TryGetValue(typeof(TConfig), out BaseLocalConfig localConfig))
            {
                config = (TConfig)localConfig;
                return true;
            }

            if (_parentService != null)
            {
                return _parentService.TryGetConfig(out config);
            }

            config = null;
            return false;
        }

        private void EnsureInitialized()
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    "LocalConfigService must be initialized by LocalConfigFeature before use.");
            }
        }
    }
}
