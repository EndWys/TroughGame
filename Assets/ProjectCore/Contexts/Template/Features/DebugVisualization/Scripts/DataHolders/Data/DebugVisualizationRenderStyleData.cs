using UnityEngine;

namespace ProjectCore.Template
{
    internal readonly struct DebugVisualizationRenderStyleData
    {
        public DebugVisualizationRenderStyleData(DebugVisualizationStyleData style)
        {
            Color = style.VisibleColor;
            FillColor = style.VisibleFillColor;
            TextSize = style.TextSize;
            LineWidth = style.LineWidth;
            DepthTest = style.DepthTest;
        }

        public Color Color { get; }
        public Color FillColor { get; }
        public float TextSize { get; }
        public float LineWidth { get; }
        public bool DepthTest { get; }
    }
}
