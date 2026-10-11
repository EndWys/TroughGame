using System;
using System.Collections.Generic;

namespace ProjectCore.Template
{
    public static class LocalConfigCatalogValidation
    {
        public static void Validate(LocalConfigCatalogConfig catalog)
        {
            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            IReadOnlyList<BaseLocalConfig> configs = catalog.Configs;
            if (configs == null)
            {
                throw new InvalidOperationException(
                    $"Local config catalog '{catalog.name}' has no config collection.");
            }

            var configTypes = new HashSet<Type>();
            for (int i = 0; i < configs.Count; i++)
            {
                BaseLocalConfig config = configs[i];
                if (config == null)
                {
                    throw new InvalidOperationException(
                        $"Local config catalog '{catalog.name}' contains a null entry at index {i}.");
                }

                Type configType = config.GetType();
                if (!configTypes.Add(configType))
                {
                    throw new InvalidOperationException(
                        $"Local config catalog '{catalog.name}' contains duplicate config type " +
                        $"'{configType.Name}'.");
                }
            }
        }
    }
}
