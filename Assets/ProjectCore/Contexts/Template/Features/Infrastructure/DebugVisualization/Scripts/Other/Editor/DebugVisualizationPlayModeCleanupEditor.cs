#if UNITY_EDITOR
using UnityEditor;

namespace ProjectCore.Template
{
    [InitializeOnLoad]
    public static class DebugVisualizationPlayModeCleanupEditor
    {
        static DebugVisualizationPlayModeCleanupEditor()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state is PlayModeStateChange.ExitingPlayMode or PlayModeStateChange.EnteredEditMode)
                DebugVisualizationUtility.Reset();
        }
    }
}
#endif
