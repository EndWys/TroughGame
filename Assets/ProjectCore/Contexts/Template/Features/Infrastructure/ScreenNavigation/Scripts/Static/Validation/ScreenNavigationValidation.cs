using Domain;
using System;
using System.Collections.Generic;

namespace ProjectCore.Template
{
    public static class ScreenNavigationValidation
    {
        public static void ValidateCatalog(ScreenCatalogConfig screenCatalog)
        {
            if (screenCatalog == null)
            {
                throw new ArgumentNullException(nameof(screenCatalog));
            }

            HashSet<Type> registeredScreenTypes = new HashSet<Type>();

            foreach (BaseScreenDefinition definition in screenCatalog.Definitions)
            {
                ValidateDefinition(definition);

                if (!registeredScreenTypes.Add(definition.ScreenType))
                {
                    throw new InvalidOperationException(
                        $"Screen catalog contains duplicate {definition.ScreenType.Name} definitions.");
                }
            }
        }

        public static Result ValidateSettings(
            BaseScreenDefinition definition,
            IScreenSettings settings)
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
                : Result.Failure(ScreenNavigationErrors.InvalidSettings(
                    definition.SettingsType,
                    settings.GetType()));
        }

        public static Result ValidateRoot(BaseScreenDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return definition.ParentScreenType == null
                ? Result.Success()
                : Result.Failure(ScreenNavigationErrors.InvalidRoot(definition.ScreenType));
        }

        public static Result ValidateParent(
            BaseScreenDefinition definition,
            Type currentScreenType)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            return definition.ParentScreenType == currentScreenType
                ? Result.Success()
                : Result.Failure(ScreenNavigationErrors.InvalidParent(
                    definition.ScreenType,
                    definition.ParentScreenType));
        }

        private static void ValidateDefinition(BaseScreenDefinition definition)
        {
            if (definition == null)
            {
                throw new InvalidOperationException("Screen catalog contains an empty definition.");
            }

            if (definition.Layout == null)
            {
                throw new InvalidOperationException(
                    $"Screen definition {definition.name} requires a UXML layout.");
            }
        }
    }
}
