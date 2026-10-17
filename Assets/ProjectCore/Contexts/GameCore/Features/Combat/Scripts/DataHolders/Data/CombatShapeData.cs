using UnityEngine;

namespace ProjectCore.GameCore
{
    public readonly struct CombatShapeData
    {
        public CombatShapeData(
            CombatShapeType shapeType,
            Vector2 center,
            Vector2 halfExtents,
            float radius,
            float rotationDegrees)
        {
            ShapeType = shapeType;
            Center = center;
            HalfExtents = halfExtents;
            Radius = radius;
            RotationDegrees = rotationDegrees;
        }

        public CombatShapeType ShapeType { get; }
        public Vector2 Center { get; }
        public Vector2 HalfExtents { get; }
        public float Radius { get; }
        public float RotationDegrees { get; }
    }
}
