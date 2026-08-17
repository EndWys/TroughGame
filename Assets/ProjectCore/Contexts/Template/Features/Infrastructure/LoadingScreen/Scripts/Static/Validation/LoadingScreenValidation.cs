using Domain;
using System;
using System.Collections.Generic;

namespace ProjectCore.Template
{
    public static class LoadingScreenValidation
    {
        public static void ValidateCatalog(LoadingScreenCatalogConfig loadingScreenCatalog)
        {
            if (loadingScreenCatalog == null)
            {
                throw new ArgumentNullException(nameof(loadingScreenCatalog));
            }

            if (loadingScreenCatalog.DefaultDefinition == null)
            {
                throw new InvalidOperationException(
                    "Loading screen catalog requires a default definition.");
            }

            HashSet<Type> registeredScreenTypes = new HashSet<Type>();
            bool containsDefaultDefinition = false;

            foreach (BaseLoadingScreenDefinition definition in loadingScreenCatalog.Definitions)
            {
                ValidateDefinition(definition);

                if (!registeredScreenTypes.Add(definition.ScreenType))
                {
                    throw new InvalidOperationException(
                        $"Loading screen catalog contains duplicate " +
                        $"{definition.ScreenType.Name} definitions.");
                }

                containsDefaultDefinition |=
                    ReferenceEquals(definition, loadingScreenCatalog.DefaultDefinition);
            }

            if (!containsDefaultDefinition)
            {
                throw new InvalidOperationException(
                    "The default loading screen definition must be registered in the catalog.");
            }

            if (loadingScreenCatalog.DefaultDefinition.SettingsType !=
                typeof(EmptyLoadingScreenSettings))
            {
                throw new InvalidOperationException(
                    "The default loading screen must use EmptyLoadingScreenSettings.");
            }
        }

        public static Result ValidateOperation(
            BaseLoadingScreenDefinition definition,
            ILoadingScreenSettings settings)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            return definition.SettingsType.IsInstanceOfType(settings)
                ? Result.Success()
                : Result.Failure(LoadingScreenErrors.InvalidSettings(
                    definition.SettingsType,
                    settings.GetType()));
        }

        private static void ValidateDefinition(BaseLoadingScreenDefinition definition)
        {
            if (definition == null)
            {
                throw new InvalidOperationException(
                    "Loading screen catalog contains an empty definition.");
            }

            if (definition.Layout == null)
            {
                throw new InvalidOperationException(
                    $"Loading screen definition {definition.name} requires a UXML layout.");
            }
        }
    }
}
