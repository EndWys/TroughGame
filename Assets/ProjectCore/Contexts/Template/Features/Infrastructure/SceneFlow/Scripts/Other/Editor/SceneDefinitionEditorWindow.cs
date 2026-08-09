using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ProjectCore.Template
{
    public sealed class SceneDefinitionEditorWindow : EditorWindow
    {
        private const string DefaultScenesDirectory =
            "Assets/ProjectCore/Contexts/Prototype/GraphicResources/Scenes";

        private BaseSceneDefinition _existingDefinition;
        private SceneCatalogConfig _sceneCatalog;
        private SceneAsset _sceneAsset;
        private string _newSceneFileName = "Scene_New";
        private string _newSceneDirectory = DefaultScenesDirectory;
        private int _selectedDefinitionTypeIndex;
        private int _selectedMode;
        private string _statusMessage;
        private Type[] _definitionTypes = Array.Empty<Type>();

        [MenuItem("Tools/Scene Flow/Scene Definition Window")]
        public static void Open()
        {
            GetWindow<SceneDefinitionEditorWindow>("Scene Definition");
        }

        private void OnEnable()
        {
            _definitionTypes = TypeCache.GetTypesDerivedFrom<BaseSceneDefinition>()
                .Where(type => !type.IsAbstract)
                .Where(type => !type.ContainsGenericParameters)
                .Where(type => typeof(ScriptableObject).IsAssignableFrom(type))
                .OrderBy(type => type.FullName)
                .ToArray();

            if (_sceneCatalog == null)
            {
                string[] catalogGuids = AssetDatabase.FindAssets("t:SceneCatalogConfig");

                if (catalogGuids.Length == 1)
                {
                    string catalogPath = AssetDatabase.GUIDToAssetPath(catalogGuids[0]);
                    _sceneCatalog = AssetDatabase.LoadAssetAtPath<SceneCatalogConfig>(catalogPath);
                }
            }
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Scene Definition", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Creates or registers a typed SceneDefinition, keeps it beside its Unity scene, " +
                "and synchronizes the scene Build Settings index.",
                MessageType.Info);

            _sceneCatalog = (SceneCatalogConfig)EditorGUILayout.ObjectField(
                "Scene Catalog",
                _sceneCatalog,
                typeof(SceneCatalogConfig),
                false);

            _selectedMode = GUILayout.Toolbar(
                _selectedMode,
                new[] { "Existing Scene", "New Scene" });

            EditorGUILayout.Space();

            if (_selectedMode == 0)
            {
                DrawExistingSceneMode();
            }
            else
            {
                DrawNewSceneMode();
            }

            if (!string.IsNullOrEmpty(_statusMessage))
            {
                EditorGUILayout.HelpBox(_statusMessage, MessageType.None);
            }
        }

        private void DrawExistingSceneMode()
        {
            _sceneAsset = (SceneAsset)EditorGUILayout.ObjectField(
                "Scene",
                _sceneAsset,
                typeof(SceneAsset),
                false);

            _existingDefinition = (BaseSceneDefinition)EditorGUILayout.ObjectField(
                "Existing Definition",
                _existingDefinition,
                typeof(BaseSceneDefinition),
                false);

            if (_existingDefinition == null)
            {
                DrawDefinitionTypeSelector();
            }

            EditorGUILayout.Space();

            if (GUILayout.Button(_existingDefinition == null
                    ? "Create And Register Definition"
                    : "Register Existing Definition"))
            {
                RegisterExistingScene();
            }
        }

        private void DrawNewSceneMode()
        {
            _newSceneDirectory = EditorGUILayout.TextField("Scene Directory", _newSceneDirectory);
            _newSceneFileName = EditorGUILayout.TextField("Scene File Name", _newSceneFileName);
            DrawDefinitionTypeSelector();

            EditorGUILayout.HelpBox(
                "The new scene is intentionally empty. Add its SceneContext and context installer " +
                "before using it as a gameplay scene.",
                MessageType.Warning);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Scene And Definition"))
            {
                CreateNewScene();
            }
        }

        private void DrawDefinitionTypeSelector()
        {
            if (_definitionTypes.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "No concrete BaseSceneDefinition types are available.",
                    MessageType.Error);
                return;
            }

            string[] typeNames = _definitionTypes.Select(type => type.FullName).ToArray();
            _selectedDefinitionTypeIndex = EditorGUILayout.Popup(
                "Definition Type",
                _selectedDefinitionTypeIndex,
                typeNames);
        }

        private void RegisterExistingScene()
        {
            if (!ValidateSceneAndCatalog())
            {
                return;
            }

            BaseSceneDefinition definition = _existingDefinition ?? CreateDefinitionAsset();

            if (definition == null)
            {
                return;
            }

            if (!CanRegisterDefinition(definition))
            {
                return;
            }

            if (!MoveDefinitionBesideScene(definition))
            {
                return;
            }

            AddSceneToBuildSettings();
            definition.SetSceneAsset(_sceneAsset);
            RegisterDefinition(definition);
            SaveChanges(definition);

            _existingDefinition = definition;
            _statusMessage = $"Registered {definition.name} for {_sceneAsset.name}.";
        }

        private void CreateNewScene()
        {
            if (_sceneCatalog == null)
            {
                _statusMessage = "Assign a SceneCatalogConfig before creating a scene.";
                return;
            }

            if (_definitionTypes.Length == 0)
            {
                _statusMessage = "Create a concrete BaseSceneDefinition type first.";
                return;
            }

            if (!AssetDatabase.IsValidFolder(_newSceneDirectory))
            {
                _statusMessage = "Scene Directory must be an existing folder below Assets.";
                return;
            }

            string sceneFileName = _newSceneFileName.EndsWith(".unity", StringComparison.Ordinal)
                ? _newSceneFileName
                : $"{_newSceneFileName}.unity";
            string scenePath = Path.Combine(_newSceneDirectory, sceneFileName).Replace('\\', '/');

            if (File.Exists(scenePath))
            {
                _statusMessage = $"Scene already exists at {scenePath}.";
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                _statusMessage = "Scene creation was cancelled.";
                return;
            }

            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();

            _sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            _existingDefinition = null;
            RegisterExistingScene();
        }

        private bool ValidateSceneAndCatalog()
        {
            if (_sceneAsset == null)
            {
                _statusMessage = "Assign a Unity scene.";
                return false;
            }

            if (_sceneCatalog == null)
            {
                _statusMessage = "Assign a SceneCatalogConfig.";
                return false;
            }

            if (_existingDefinition == null && _definitionTypes.Length == 0)
            {
                _statusMessage = "Create a concrete BaseSceneDefinition type first.";
                return false;
            }

            return true;
        }

        private BaseSceneDefinition CreateDefinitionAsset()
        {
            Type definitionType = _definitionTypes[_selectedDefinitionTypeIndex];
            BaseSceneDefinition definition =
                ScriptableObject.CreateInstance(definitionType) as BaseSceneDefinition;

            if (definition == null)
            {
                _statusMessage = $"Could not create {definitionType.Name}.";
                return null;
            }

            if (!CanRegisterDefinition(definition))
            {
                DestroyImmediate(definition);
                return null;
            }

            string definitionPath = GetDefinitionAssetPath();

            if (File.Exists(definitionPath))
            {
                _statusMessage = $"Definition already exists at {definitionPath}.";
                DestroyImmediate(definition);
                return null;
            }

            AssetDatabase.CreateAsset(definition, definitionPath);
            return definition;
        }

        private bool MoveDefinitionBesideScene(BaseSceneDefinition definition)
        {
            string sceneDirectory = Path.GetDirectoryName(AssetDatabase.GetAssetPath(_sceneAsset));
            string definitionPath = AssetDatabase.GetAssetPath(definition);
            string targetPath = Path.Combine(sceneDirectory, Path.GetFileName(definitionPath))
                .Replace('\\', '/');

            if (definitionPath == targetPath)
            {
                return true;
            }

            if (File.Exists(targetPath))
            {
                _statusMessage = $"A definition already exists at {targetPath}.";
                return false;
            }

            string moveError = AssetDatabase.MoveAsset(definitionPath, targetPath);

            if (!string.IsNullOrEmpty(moveError))
            {
                _statusMessage = moveError;
                return false;
            }

            return true;
        }

        private void AddSceneToBuildSettings()
        {
            string scenePath = AssetDatabase.GetAssetPath(_sceneAsset);
            var buildScenes = EditorBuildSettings.scenes.ToList();

            if (buildScenes.All(scene => scene.path != scenePath))
            {
                buildScenes.Add(new EditorBuildSettingsScene(scenePath, true));
                EditorBuildSettings.scenes = buildScenes.ToArray();
            }
        }

        private void RegisterDefinition(BaseSceneDefinition definition)
        {
            SerializedObject serializedCatalog = new SerializedObject(_sceneCatalog);
            SerializedProperty definitions = serializedCatalog.FindProperty("_definitions");

            for (int index = 0; index < definitions.arraySize; index++)
            {
                BaseSceneDefinition registered =
                    definitions.GetArrayElementAtIndex(index).objectReferenceValue as BaseSceneDefinition;

                if (registered == definition)
                {
                    return;
                }
            }

            definitions.InsertArrayElementAtIndex(definitions.arraySize);
            definitions.GetArrayElementAtIndex(definitions.arraySize - 1).objectReferenceValue = definition;
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();
        }

        private bool CanRegisterDefinition(BaseSceneDefinition definition)
        {
            foreach (BaseSceneDefinition registered in _sceneCatalog.Definitions)
            {
                if (registered == definition)
                {
                    return true;
                }

                if (registered != null && registered.SceneType == definition.SceneType)
                {
                    _statusMessage =
                        $"{registered.name} already registers {definition.SceneType.Name}.";
                    return false;
                }
            }

            return true;
        }

        private void SaveChanges(BaseSceneDefinition definition)
        {
            EditorUtility.SetDirty(definition);
            EditorUtility.SetDirty(_sceneCatalog);
            AssetDatabase.SaveAssets();
        }

        private string GetDefinitionAssetPath()
        {
            string scenePath = AssetDatabase.GetAssetPath(_sceneAsset);
            string sceneDirectory = Path.GetDirectoryName(scenePath);
            string sceneName = Path.GetFileNameWithoutExtension(scenePath);
            string sceneIdentity = sceneName.StartsWith("Scene_", StringComparison.Ordinal)
                ? sceneName.Substring("Scene_".Length)
                : sceneName;
            string[] identityParts = sceneIdentity.Split('_');
            string context = identityParts[0];
            string detail = identityParts.Length > 1
                ? string.Join("_", identityParts.Skip(1))
                : context;
            string definitionName = $"Config_{context}_SceneFlow_{detail}.asset";

            return Path.Combine(sceneDirectory, definitionName).Replace('\\', '/');
        }
    }
}
