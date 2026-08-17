using UnityEngine;

namespace ProjectCore.Template
{
    [CreateAssetMenu(
        fileName = "Config_Template_DebugVisualization_Style",
        menuName = "SO/Template/DebugVisualization/Style")]
    public sealed class DebugVisualizationStyleConfig : ScriptableObject
    {
        [SerializeField] private DebugVisualizationStyleData _data = DebugVisualizationStyleData.Default;

        public DebugVisualizationStyleData Data => _data;
    }
}
