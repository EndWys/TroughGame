using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public static class CombatGeometryUtility
    {
        // Allow float-authored touching coordinates to survive rotation roundoff.
        private const double RelativeTolerance = 0.0000002d;

        public static CombatShapeData CreateWorldShape(
            in CombatShapeSettings settings,
            Vector2 origin,
            Vector2 forward)
        {
            ValidateFinite(origin, nameof(origin));
            ValidateFinite(forward, nameof(forward));
            CombatShapeValidation.ValidateSettings(settings);

            double length = Math.Sqrt((double)forward.x * forward.x + (double)forward.y * forward.y);
            if (length == 0d)
            {
                throw new ArgumentException("Forward must be nonzero.", nameof(forward));
            }

            double forwardX = forward.x / length;
            double forwardY = forward.y / length;
            var center = new Vector2(
                (float)(origin.x + forwardX * settings.Offset.x - forwardY * settings.Offset.y),
                (float)(origin.y + forwardY * settings.Offset.x + forwardX * settings.Offset.y));
            ValidateFinite(center, nameof(settings));

            float rotation = (float)(Math.Atan2(forwardY, forwardX) * 180d / Math.PI +
                settings.RotationDegrees);
            var shape = new CombatShapeData(
                settings.ShapeType,
                center,
                settings.Size * 0.5f,
                settings.Radius,
                rotation);
            CombatShapeValidation.ValidateWorldShape(shape);
            return shape;
        }

        public static bool Overlaps(in CombatShapeData first, in CombatShapeData second)
        {
            CombatShapeValidation.ValidateWorldShape(first);
            CombatShapeValidation.ValidateWorldShape(second);

            if (first.ShapeType == CombatShapeType.Circle)
            {
                return second.ShapeType == CombatShapeType.Circle
                    ? CirclesOverlap(first, second)
                    : CircleBoxOverlap(first, second);
            }

            return second.ShapeType == CombatShapeType.Circle
                ? CircleBoxOverlap(second, first)
                : BoxesOverlap(first, second);
        }

        private static bool CirclesOverlap(in CombatShapeData first, in CombatShapeData second)
        {
            double deltaX = (double)first.Center.x - second.Center.x;
            double deltaY = (double)first.Center.y - second.Center.y;
            double radius = (double)first.Radius + second.Radius;
            return Within(deltaX * deltaX + deltaY * deltaY, radius * radius);
        }

        private static bool CircleBoxOverlap(in CombatShapeData circle, in CombatShapeData box)
        {
            var axes = new BoxAxes(box.RotationDegrees);
            double deltaX = (double)circle.Center.x - box.Center.x;
            double deltaY = (double)circle.Center.y - box.Center.y;
            double localX = deltaX * axes.ForwardX + deltaY * axes.ForwardY;
            double localY = deltaX * axes.LeftX + deltaY * axes.LeftY;
            double outsideX = Math.Max(Math.Abs(localX) - box.HalfExtents.x, 0d);
            double outsideY = Math.Max(Math.Abs(localY) - box.HalfExtents.y, 0d);
            return Within(outsideX * outsideX + outsideY * outsideY, (double)circle.Radius * circle.Radius);
        }

        private static bool BoxesOverlap(in CombatShapeData first, in CombatShapeData second)
        {
            var firstAxes = new BoxAxes(first.RotationDegrees);
            var secondAxes = new BoxAxes(second.RotationDegrees);
            return OverlapsOnAxis(first, second, firstAxes, secondAxes, firstAxes.ForwardX, firstAxes.ForwardY)
                && OverlapsOnAxis(first, second, firstAxes, secondAxes, firstAxes.LeftX, firstAxes.LeftY)
                && OverlapsOnAxis(first, second, firstAxes, secondAxes, secondAxes.ForwardX, secondAxes.ForwardY)
                && OverlapsOnAxis(first, second, firstAxes, secondAxes, secondAxes.LeftX, secondAxes.LeftY);
        }

        private static bool OverlapsOnAxis(
            in CombatShapeData first,
            in CombatShapeData second,
            in BoxAxes firstAxes,
            in BoxAxes secondAxes,
            double axisX,
            double axisY)
        {
            double deltaX = (double)second.Center.x - first.Center.x;
            double deltaY = (double)second.Center.y - first.Center.y;
            double distance = Math.Abs(deltaX * axisX + deltaY * axisY);
            double firstRadius = ProjectionRadius(first.HalfExtents, firstAxes, axisX, axisY);
            double secondRadius = ProjectionRadius(second.HalfExtents, secondAxes, axisX, axisY);
            return Within(distance, firstRadius + secondRadius);
        }

        private static double ProjectionRadius(Vector2 halfExtents, in BoxAxes axes, double axisX, double axisY)
        {
            return halfExtents.x * Math.Abs(axes.ForwardX * axisX + axes.ForwardY * axisY)
                + halfExtents.y * Math.Abs(axes.LeftX * axisX + axes.LeftY * axisY);
        }

        private static bool Within(double distance, double limit)
        {
            return distance <= limit + RelativeTolerance * Math.Max(distance, limit);
        }

        private static void ValidateFinite(Vector2 value, string parameterName)
        {
            ValidateFinite(value.x, parameterName);
            ValidateFinite(value.y, parameterName);
        }

        private static void ValidateFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                throw new ArgumentException("Shape values must be finite.", parameterName);
            }
        }

        private readonly struct BoxAxes
        {
            public BoxAxes(float rotationDegrees)
            {
                double angle = (rotationDegrees % 360d) * Math.PI / 180d;
                ForwardX = Math.Cos(angle);
                ForwardY = Math.Sin(angle);
            }

            public double ForwardX { get; }
            public double ForwardY { get; }
            public double LeftX => -ForwardY;
            public double LeftY => ForwardX;
        }
    }
}
