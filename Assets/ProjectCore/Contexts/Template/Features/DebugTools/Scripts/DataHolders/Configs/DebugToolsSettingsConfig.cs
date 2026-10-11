using UnityEngine;

namespace ProjectCore.Template
{
    [CreateAssetMenu(
        fileName = "Config_Template_DebugTools_Settings",
        menuName = "SO/Template/DebugTools/Settings")]
    public sealed class DebugToolsSettingsConfig : ScriptableObject
    {
        [Header("Build Availability")]
        [UnityEngine.Serialization.FormerlySerializedAs("_enabledInEditor")]
        [SerializeField] private bool _isEnabledInEditor = true;
        [UnityEngine.Serialization.FormerlySerializedAs("_enabledInDevelopmentBuild")]
        [SerializeField] private bool _isEnabledInDevelopmentBuild = true;
        [UnityEngine.Serialization.FormerlySerializedAs("_enabledInReleaseBuild")]
        [SerializeField] private bool _isEnabledInReleaseBuild;
        [UnityEngine.Serialization.FormerlySerializedAs("_enabledOnDedicatedServer")]
        [SerializeField] private bool _isEnabledOnDedicatedServer;

        [Header("Modules")]
        [SerializeField] private bool _consoleEnabled = true;
        [SerializeField] private bool _cheatsEnabled = true;
        [SerializeField] private bool _visualizationEnabled = true;

        [Header("Console")]
        [SerializeField] private DebugConsoleSettings _consoleSettings = new();

        public bool IsEnabledInEditor => _isEnabledInEditor;
        public bool IsEnabledInDevelopmentBuild => _isEnabledInDevelopmentBuild;
        public bool IsEnabledInReleaseBuild => _isEnabledInReleaseBuild;
        public bool IsEnabledOnDedicatedServer => _isEnabledOnDedicatedServer;
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
