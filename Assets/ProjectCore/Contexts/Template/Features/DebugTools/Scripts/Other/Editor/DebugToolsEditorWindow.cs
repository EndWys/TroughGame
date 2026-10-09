#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class DebugToolsEditorWindow : EditorWindow
    {
        private const string ProjectContextPath = "Assets/ProjectCore/Resources/ProjectContext.prefab";

        private DebugToolsSettingsConfig _settingsConfig;
        private DebugVisualizationSettingsConfig _visualizationSettingsConfig;
        private SerializedObject _serializedSettings;
        private SerializedObject _serializedVisualizationSettings;
        private Vector2 _scrollPosition;

        [MenuItem("ProjectCore/Debug Tools")]
        private static void Open()
        {
            GetWindow<DebugToolsEditorWindow>("Debug Tools");
        }

        private void OnEnable()
        {
            FindSettingsConfig();
        }

        private void OnInspectorUpdate()
        {
            if (EditorApplication.isPlaying)
                Repaint();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Debug Tools", EditorStyles.boldLabel);

            DrawRuntimeControls();

            EditorGUI.BeginChangeCheck();
            var selectedConfig = (DebugToolsSettingsConfig)EditorGUILayout.ObjectField(
                "Settings",
                _settingsConfig,
                typeof(DebugToolsSettingsConfig),
                false);
            if (EditorGUI.EndChangeCheck())
                SetSettingsConfig(selectedConfig);

            if (_serializedSettings == null)
            {
                EditorGUILayout.HelpBox(
                    "Assign a DebugToolsSettingsConfig or configure it on ProjectContext.prefab.",
                    MessageType.Warning);
                return;
            }

            _serializedSettings.Update();
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawBuildAvailability();
            DrawModules();
            DrawConsoleSettings();
            DrawVisualizationSettings();

            EditorGUILayout.EndScrollView();
            _serializedSettings.ApplyModifiedProperties();

            EditorGUILayout.Space(6f);
            DrawActions();
        }

        private void DrawBuildAvailability()
        {
            EditorGUILayout.LabelField("Build Availability", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                _serializedSettings.FindProperty("_enabledInEditor"), new GUIContent("Editor"));
            EditorGUILayout.PropertyField(
                _serializedSettings.FindProperty("_enabledInDevelopmentBuild"),
                new GUIContent("Development Build"));
            EditorGUILayout.PropertyField(
                _serializedSettings.FindProperty("_enabledInReleaseBuild"), new GUIContent("Release Build"));
            EditorGUILayout.PropertyField(
                _serializedSettings.FindProperty("_enabledOnDedicatedServer"),
                new GUIContent("Dedicated Server"));

            if (_serializedSettings.FindProperty("_enabledInReleaseBuild").boolValue)
            {
                EditorGUILayout.HelpBox(
                    "Debug tools are enabled in release builds.",
                    MessageType.Warning);
            }

            EditorGUILayout.Space(8f);
        }

        private void DrawRuntimeControls()
        {
            if (!EditorApplication.isPlaying)
                return;

            DebugToolsFeature runtimeFeature = UnityEngine.Object.FindFirstObjectByType<DebugToolsFeature>();
            IDebugToolsService service = runtimeFeature != null
                ? runtimeFeature.EditorService
                : null;

            EditorGUILayout.LabelField("Current Session", EditorStyles.boldLabel);

            if (service == null)
            {
                EditorGUILayout.HelpBox("Debug Tools are not initialized.", MessageType.Info);
                EditorGUILayout.Space(8f);
                return;
            }

            DrawRuntimeTool(service, DebugToolTypes.Console, "Runtime Console");
            DrawRuntimeTool(service, DebugToolTypes.Cheats, "Cheats");
            DrawRuntimeTool(service, DebugToolTypes.Visualization, "Debug Visualization");
            EditorGUILayout.Space(8f);
        }

        private static void DrawRuntimeTool(
            IDebugToolsService service,
            DebugToolTypes tool,
            string label)
        {
            bool isAllowed = service.IsAllowed(tool);
            bool isEnabled = service.IsEnabled(tool);

            using (new EditorGUI.DisabledScope(!isAllowed))
            {
                bool nextState = EditorGUILayout.Toggle(label, isEnabled);
                if (nextState == isEnabled)
                    return;

                var result = nextState
                    ? service.Enable(tool)
                    : service.Disable(tool);

                if (result.IsFailure)
                    Debug.LogError(result.FirstError.Message);
            }
        }

        private void DrawModules()
        {
            EditorGUILayout.LabelField("Modules", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(
                _serializedSettings.FindProperty("_consoleEnabled"), new GUIContent("Runtime Console"));
            EditorGUILayout.PropertyField(
                _serializedSettings.FindProperty("_cheatsEnabled"), new GUIContent("Cheats"));
            EditorGUILayout.PropertyField(
                _serializedSettings.FindProperty("_visualizationEnabled"),
                new GUIContent("Debug Visualization"));
            EditorGUILayout.Space(8f);
        }

        private void DrawConsoleSettings()
        {
            EditorGUILayout.LabelField("Console", EditorStyles.boldLabel);
            SerializedProperty consoleSettings = _serializedSettings.FindProperty("_consoleSettings");
            EditorGUILayout.PropertyField(
                consoleSettings.FindPropertyRelative("_openOnStart"), new GUIContent("Open on Start"));
            EditorGUILayout.PropertyField(
                consoleSettings.FindPropertyRelative("_toggleKey"), new GUIContent("Toggle Key"));
            EditorGUILayout.PropertyField(
                consoleSettings.FindPropertyRelative("_maxLogs"), new GUIContent("Max Logs"));
            EditorGUILayout.Space(8f);
        }

        private void DrawVisualizationSettings()
        {
            EditorGUILayout.LabelField("Visualization", EditorStyles.boldLabel);

            if (_serializedVisualizationSettings == null)
            {
                EditorGUILayout.HelpBox(
                    "Debug Visualization settings are not assigned on ProjectContext.",
                    MessageType.Warning);
                return;
            }

            _serializedVisualizationSettings.Update();
            EditorGUILayout.PropertyField(
                _serializedVisualizationSettings.FindProperty("_maxTransientDraws"),
                new GUIContent("Max Draws"));
            _serializedVisualizationSettings.ApplyModifiedProperties();
        }

        private void DrawActions()
        {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Validate Configuration"))
                ValidateConfiguration();

            if (GUILayout.Button("Select Config"))
                Selection.activeObject = _settingsConfig;

            if (GUILayout.Button("Select Visualization Config"))
                Selection.activeObject = _visualizationSettingsConfig;

            if (GUILayout.Button("Select ProjectContext"))
                Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectContextPath);

            EditorGUILayout.EndHorizontal();
        }

        private void FindSettingsConfig()
        {
            GameObject projectContext = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectContextPath);
            DebugToolsFeature feature = projectContext != null
                ? projectContext.GetComponent<DebugToolsFeature>()
                : null;
            DebugVisualizationFeature visualizationFeature = projectContext != null
                ? projectContext.GetComponent<DebugVisualizationFeature>()
                : null;

            if (feature != null)
            {
                var serializedFeature = new SerializedObject(feature);
                SetSettingsConfig(
                    serializedFeature.FindProperty("_settingsConfig").objectReferenceValue
                    as DebugToolsSettingsConfig);
            }

            if (visualizationFeature != null)
            {
                var serializedFeature = new SerializedObject(visualizationFeature);
                SetVisualizationSettingsConfig(
                    serializedFeature.FindProperty("_settingsConfig").objectReferenceValue
                    as DebugVisualizationSettingsConfig);
            }
        }

        private void SetSettingsConfig(DebugToolsSettingsConfig settingsConfig)
        {
            _settingsConfig = settingsConfig;
            _serializedSettings = settingsConfig != null
                ? new SerializedObject(settingsConfig)
                : null;
        }

        private void SetVisualizationSettingsConfig(
            DebugVisualizationSettingsConfig settingsConfig)
        {
            _visualizationSettingsConfig = settingsConfig;
            _serializedVisualizationSettings = settingsConfig != null
                ? new SerializedObject(settingsConfig)
                : null;
        }

        private void ValidateConfiguration()
        {
            GameObject projectContext = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectContextPath);
            if (projectContext == null)
            {
                Debug.LogError($"ProjectContext prefab was not found at {ProjectContextPath}.");
                return;
            }

            bool isValid = _settingsConfig != null
                           && _visualizationSettingsConfig != null
                           && projectContext.GetComponent<DebugToolsFeatureGroup>() != null
                           && projectContext.GetComponent<DebugToolsFeature>() != null
                           && projectContext.GetComponent<DebugConsoleFeature>() != null
                           && projectContext.GetComponent<DebugVisualizationFeature>() != null;

            if (isValid)
                Debug.Log("Debug Tools configuration is valid.");
            else
                Debug.LogError(
                    "Debug Tools configuration is incomplete. Check ProjectContext and the settings asset.");
        }
    }
}
#endif
