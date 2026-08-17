using System;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ProjectCore.Template
{
    public abstract class BaseSceneDefinition : ScriptableObject
    {
        [SerializeField] private int _buildIndex = -1;

#if UNITY_EDITOR
        [SerializeField] private SceneAsset _sceneAsset;
#endif

        public int BuildIndex => _buildIndex;
        public abstract Type SceneType { get; }
        public abstract Type SettingsType { get; }

#if UNITY_EDITOR
        public SceneAsset SceneAsset => _sceneAsset;

        public void SetSceneAsset(SceneAsset sceneAsset)
        {
            _sceneAsset = sceneAsset ?? throw new ArgumentNullException(nameof(sceneAsset));
            SyncBuildIndex();
        }

        private void OnValidate()
        {
            if (_sceneAsset != null)
            {
                SyncBuildIndex();
            }
        }

        private void SyncBuildIndex()
        {
            string scenePath = AssetDatabase.GetAssetPath(_sceneAsset);
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

            for (int index = 0; index < buildScenes.Length; index++)
            {
                if (buildScenes[index].enabled && buildScenes[index].path == scenePath)
                {
                    _buildIndex = index;
                    return;
                }
            }

            _buildIndex = -1;
        }
#endif
    }
}
