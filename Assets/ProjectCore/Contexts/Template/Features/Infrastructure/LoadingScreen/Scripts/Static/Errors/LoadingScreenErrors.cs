using Domain;
using System;

namespace ProjectCore.Template
{
    public static class LoadingScreenErrors
    {
        public static Error DefinitionNotRegistered(Type screenType)
        {
            return new Error(
                "LoadingScreen.DefinitionNotRegistered",
                $"Loading screen definition {screenType.Name} is not registered.");
        }

        public static Error InvalidSettings(Type expectedType, Type actualType)
        {
            return new Error(
                "LoadingScreen.InvalidSettings",
                $"Expected {expectedType.Name} settings but received {actualType.Name}.");
        }

        public static Error AlreadyVisible(Type screenType)
        {
            return new Error(
                "LoadingScreen.AlreadyVisible",
                $"Loading screen {screenType.Name} is already visible.");
        }

        public static Error TransitionInProgress()
        {
            return new Error(
                "LoadingScreen.TransitionInProgress",
                "A loading screen transition is already in progress.");
        }

        public static Error ContextDisposed(Type screenType)
        {
            return new Error(
                "LoadingScreen.ContextDisposed",
                $"Loading screen {screenType.Name} was interrupted because its context was disposed.");
        }

        public static Error LifecycleFailed(Type screenType, Exception exception)
        {
            return new Error(
                "LoadingScreen.LifecycleFailed",
                $"Loading screen {screenType.Name} lifecycle failed.",
                exception.ToString());
        }
    }
}
