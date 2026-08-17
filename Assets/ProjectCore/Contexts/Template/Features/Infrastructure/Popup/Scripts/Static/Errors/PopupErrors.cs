using Domain;
using System;

namespace ProjectCore.Template
{
    public static class PopupErrors
    {
        public static Error DefinitionNotRegistered(Type popupType)
        {
            return new Error(
                "Popup.DefinitionNotRegistered",
                $"Popup definition {popupType.Name} is not registered.");
        }

        public static Error InvalidPayload(Type expectedType, Type actualType)
        {
            return new Error(
                "Popup.InvalidPayload",
                $"Expected {expectedType.Name} payload but received {actualType.Name}.");
        }

        public static Error InvalidResponse(Type expectedType, Type actualType)
        {
            return new Error(
                "Popup.InvalidResponse",
                $"Expected {expectedType.Name} response but received {actualType.Name}.");
        }

        public static Error Dismissed(Type popupType)
        {
            return new Error(
                "Popup.Dismissed",
                $"Popup {popupType.Name} was dismissed without a response.");
        }

        public static Error Aborted(Type popupType)
        {
            return new Error(
                "Popup.Aborted",
                $"Popup {popupType.Name} was aborted externally.");
        }

        public static Error ContextDisposed(Type popupType)
        {
            return new Error(
                "Popup.ContextDisposed",
                $"Popup {popupType.Name} was interrupted because its context was disposed.");
        }

        public static Error LifecycleFailed(Type popupType, Exception exception)
        {
            return new Error(
                "Popup.LifecycleFailed",
                $"Popup {popupType.Name} lifecycle failed.",
                exception.ToString());
        }
    }
}
