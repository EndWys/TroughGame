using System;
using UnityEngine;

namespace ProjectCore.Template
{
    [Serializable]
    public sealed class DebugVisualizationStyleData
    {
        public Color Color = Color.white;
        public Color FillColor = new(1f, 1f, 1f, 0.12f);
        [Min(1f)] public float TextSize = 18f;
        [Min(0.001f)] public float LineWidth = 0.03f;
        [Min(0f)] public float Duration = 1f;
        public bool DepthTest = true;

        public static DebugVisualizationStyleData Default => new();

        public Color VisibleColor => Color.a > 0f
            ? Color
            : new Color(Color.r, Color.g, Color.b, 1f);

        public Color VisibleFillColor => FillColor.a > 0f
            ? FillColor
            : new Color(FillColor.r, FillColor.g, FillColor.b, 0.12f);

    }
}
