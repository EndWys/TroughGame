using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class DebugToolsFeatureGroup : BaseMonoBehaviourFeatureGroup
    {
        [SerializeField] private DebugToolsFeature _debugToolsFeature;
        [SerializeField] private DebugVisualizationFeature _debugVisualizationFeature;
        [SerializeField] private DebugConsoleFeature _debugConsoleFeature;

        protected override void AddFeatures()
        {
            AddFeature(_debugToolsFeature);
            AddFeature<CheatFeature>();
            AddFeature(_debugVisualizationFeature);
            AddFeature(_debugConsoleFeature);
        }
    }
}
