using Domain;
using System;
using System.Collections.Generic;

namespace ProjectCore.Template
{
    public static class PopupValidation
    {
        public static void ValidateCatalog(PopupCatalogConfig popupCatalog)
        {
            if (popupCatalog == null)
            {
                throw new ArgumentNullException(nameof(popupCatalog));
            }

            HashSet<Type> registeredPopupTypes = new HashSet<Type>();

            foreach (BasePopupDefinition definition in popupCatalog.Definitions)
            {
                ValidateDefinition(definition);

                if (!registeredPopupTypes.Add(definition.PopupType))
                {
                    throw new InvalidOperationException(
                        $"Popup catalog contains duplicate {definition.PopupType.Name} definitions.");
                }
            }
        }

        public static Result ValidateOperation(
            BasePopupDefinition definition,
            IPopupPayload payload,
            Type responseType)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (payload == null)
            {
                throw new ArgumentNullException(nameof(payload));
            }

            if (responseType == null)
            {
                throw new ArgumentNullException(nameof(responseType));
            }

            if (!definition.PayloadType.IsInstanceOfType(payload))
            {
                return Result.Failure(PopupErrors.InvalidPayload(
                    definition.PayloadType,
                    payload.GetType()));
            }

            return definition.ResponseType == responseType
                ? Result.Success()
                : Result.Failure(PopupErrors.InvalidResponse(
                    definition.ResponseType,
                    responseType));
        }

        private static void ValidateDefinition(BasePopupDefinition definition)
        {
            if (definition == null)
            {
                throw new InvalidOperationException("Popup catalog contains an empty definition.");
            }

            if (definition.Layout == null)
            {
                throw new InvalidOperationException(
                    $"Popup definition {definition.name} requires a UXML layout.");
            }
        }
    }
}
