using Domain;
using System;

namespace ProjectCore.Template
{
    public static class SceneFlowErrors
    {
        public static Error TransitionInProgress()
        {
            return new Error("SceneFlow.TransitionInProgress", "A scene transition is already in progress.");
        }

        public static Error DefinitionNotRegistered(Type sceneType)
        {
            return new Error(
                "SceneFlow.DefinitionNotRegistered",
                $"No scene definition is registered for {sceneType.Name}.");
        }

        public static Error InvalidSettings(Type expectedSettingsType, Type settingsType)
        {
            return new Error(
                "SceneFlow.InvalidSettings",
                $"Scene expects {expectedSettingsType.Name}, but received {settingsType.Name}.");
        }

        public static Error TransitionFailed(Exception exception)
        {
            return new Error(
                "SceneFlow.TransitionFailed",
                "The scene transition failed.",
                exception.ToString());
        }
    }
}
