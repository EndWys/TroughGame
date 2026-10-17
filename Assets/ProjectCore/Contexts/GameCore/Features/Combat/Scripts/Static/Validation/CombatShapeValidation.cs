using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public static class CombatShapeValidation
    {
        public static void ValidateSettings(in CombatShapeSettings settings)
        {
            ValidateFinite(settings.Offset, nameof(settings));
            ValidateFinite(settings.Size, nameof(settings));
            ValidateFinite(settings.Radius, nameof(settings));
            ValidateFinite(settings.RotationDegrees, nameof(settings));
            ValidateDimensions(settings.ShapeType, settings.Radius, settings.Size, nameof(settings));
        }

        public static void ValidateWorldShape(in CombatShapeData shape)
        {
            ValidateFinite(shape.Center, nameof(shape));
            ValidateFinite(shape.HalfExtents, nameof(shape));
            ValidateFinite(shape.Radius, nameof(shape));
            ValidateFinite(shape.RotationDegrees, nameof(shape));
            ValidateDimensions(shape.ShapeType, shape.Radius, shape.HalfExtents, nameof(shape));
        }

        private static void ValidateDimensions(
            CombatShapeType shapeType, float radius, Vector2 boxDimensions, string parameterName)
        {
            switch (shapeType)
            {
                case CombatShapeType.Circle when radius > 0f:
                case CombatShapeType.Box when boxDimensions.x > 0f && boxDimensions.y > 0f:
                    return;
                default:
                    throw new ArgumentException("A supported shape with positive used dimensions is required.", parameterName);
            }
        }

        private static void ValidateFinite(Vector2 value, string parameterName)
        {
            ValidateFinite(value.x, parameterName);
            ValidateFinite(value.y, parameterName);
        }

        private static void ValidateFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentException("Shape values must be finite.", parameterName);
        }
    }
}
