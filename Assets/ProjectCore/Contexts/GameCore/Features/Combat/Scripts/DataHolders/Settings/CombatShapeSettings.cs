using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    [Serializable]
    public struct CombatShapeSettings
    {
        [SerializeField] private CombatShapeType _shapeType;
        [SerializeField] private Vector2 _offset;
        [SerializeField, Min(0f)] private float _radius;
        [SerializeField] private Vector2 _size;
        [SerializeField] private float _rotationDegrees;

        public CombatShapeSettings(
            CombatShapeType shapeType,
            Vector2 offset,
            float radius,
            Vector2 size,
            float rotationDegrees)
        {
            _shapeType = shapeType;
            _offset = offset;
            _radius = radius;
            _size = size;
            _rotationDegrees = rotationDegrees;
        }

        public CombatShapeType ShapeType => _shapeType;
        public Vector2 Offset => _offset;
        public float Radius => _radius;
        public Vector2 Size => _size;
        public float RotationDegrees => _rotationDegrees;
    }
}
