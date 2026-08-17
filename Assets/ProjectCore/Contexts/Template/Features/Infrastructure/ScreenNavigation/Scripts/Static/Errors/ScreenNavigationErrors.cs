using Domain;
using System;

namespace ProjectCore.Template
{
    public static class ScreenNavigationErrors
    {
        public static Error DefinitionNotRegistered(Type screenType)
        {
            return new Error(
                "ScreenNavigation.DefinitionNotRegistered",
                $"Screen definition {screenType.Name} is not registered.");
        }

        public static Error InvalidSettings(Type expectedType, Type actualType)
        {
            return new Error(
                "ScreenNavigation.InvalidSettings",
                $"Expected {expectedType.Name} settings but received {actualType.Name}.");
        }

        public static Error InvalidRoot(Type screenType)
        {
            return new Error(
                "ScreenNavigation.InvalidRoot",
                $"Screen {screenType.Name} has a parent and cannot be opened as a root.");
        }

        public static Error InvalidParent(Type screenType, Type parentType)
        {
            return new Error(
                "ScreenNavigation.InvalidParent",
                $"Screen {screenType.Name} requires parent {parentType?.Name ?? "none"}.");
        }

        public static Error NavigationParentNotFound(Type screenType, Type parentType)
        {
            return new Error(
                "ScreenNavigation.NavigationParentNotFound",
                $"Screen {screenType.Name} requires an active {parentType.Name} navigation parent.");
        }

        public static Error CannotGoBack()
        {
            return new Error(
                "ScreenNavigation.CannotGoBack",
                "There is no previous screen in the current navigation history.");
        }

        public static Error TransitionInProgress()
        {
            return new Error(
                "ScreenNavigation.TransitionInProgress",
                "A screen transition is already in progress.");
        }

        public static Error TransitionFailed(Exception exception)
        {
            return new Error(
                "ScreenNavigation.TransitionFailed",
                "The screen transition failed.",
                exception.ToString());
        }
    }
}
