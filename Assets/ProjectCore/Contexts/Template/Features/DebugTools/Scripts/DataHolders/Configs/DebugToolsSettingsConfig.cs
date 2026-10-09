using UnityEngine;

namespace ProjectCore.Template
{
    [CreateAssetMenu(
        fileName = "Config_Template_DebugTools_Settings",
        menuName = "SO/Template/DebugTools/Settings")]
    public sealed class DebugToolsSettingsConfig : ScriptableObject
    {
        [Header("Build Availability")]
        [SerializeField] private bool _enabledInEditor = true;
        [SerializeField] private bool _enabledInDevelopmentBuild = true;
        [SerializeField] private bool _enabledInReleaseBuild;
        [SerializeField] private bool _enabledOnDedicatedServer;

        [Header("Modules")]
        [SerializeField] private bool _consoleEnabled = true;
        [SerializeField] private bool _cheatsEnabled = true;
        [SerializeField] private bool _visualizationEnabled = true;

        [Header("Console")]
        [SerializeField] private DebugConsoleSettings _consoleSettings = new();

        public bool EnabledInEditor => _enabledInEditor;
        public bool EnabledInDevelopmentBuild => _enabledInDevelopmentBuild;
        public bool EnabledInReleaseBuild => _enabledInReleaseBuild;
        public bool EnabledOnDedicatedServer => _enabledOnDedicatedServer;
        public DebugConsoleSettings ConsoleSettings => _consoleSettings;

        public bool IsInitiallyEnabled(DebugToolTypes tool)
        {
            return tool switch
            {
                DebugToolTypes.Console => _consoleEnabled,
                DebugToolTypes.Cheats => _cheatsEnabled,
                DebugToolTypes.Visualization => _visualizationEnabled,
                _ => false
            };
        }
    }
}
