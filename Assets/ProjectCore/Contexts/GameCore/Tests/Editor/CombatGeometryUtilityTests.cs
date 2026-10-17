using System;
using NUnit.Framework;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class CombatGeometryUtilityTests
    {
        [TestCase(0f, true)]
        [TestCase(1.5f, true)]
        [TestCase(2f, true)]
        [TestCase(2.001f, false)]
        public void CirclesIncludeTouchingBoundary(float separation, bool expected)
        {
            AssertSymmetric(Circle(Vector2.zero, 1f), Circle(new Vector2(separation, 0f), 1f), expected);
        }

        [TestCase(0f, 0f, true)]
        [TestCase(1.5f, 0f, true)]
        [TestCase(1.501f, 0f, false)]
        [TestCase(1.3f, 1.4f, true)]
        [TestCase(1.31f, 1.41f, false)]
        public void CircleBoxUsesClosestEdgeOrCorner(float x, float y, bool expected)
        {
            AssertSymmetric(Circle(new Vector2(x, y), 0.5f), Box(Vector2.zero, Vector2.one, 0f), expected);
        }

        [Test]
        public void CircleTouchesRotatedBoxEdgeAndCorner()
        {
            Vector2 forward = new Vector2(1f, 1f).normalized;
            Vector2 left = new(-forward.y, forward.x);
            CombatShapeData box = Box(Vector2.zero, Vector2.one, 45f);

            AssertSymmetric(Circle(forward * 1.5f, 0.5f), box, true);
            AssertSymmetric(Circle(forward * 1.3f + left * 1.4f, 0.5f), box, true);
            AssertSymmetric(Circle(forward * 1.301f + left * 1.401f, 0.5f), box, false);
        }

        [TestCase(2f, 0f, true)]
        [TestCase(2f, 2f, true)]
        [TestCase(2.001f, 0f, false)]
        [TestCase(2.001f, 2.001f, false)]
        public void BoxesIncludeTouchingEdgeAndCorner(float x, float y, bool expected)
        {
            AssertSymmetric(Box(Vector2.zero, Vector2.one, 0f), Box(new Vector2(x, y), Vector2.one, 0f), expected);
        }

        [Test]
        public void DiamondsTouchAtVertexButDoNotUseTheirAxisAlignedBounds()
        {
            CombatShapeData diamond = Box(Vector2.zero, Vector2.one, 45f);
            AssertSymmetric(diamond, Box(new Vector2(2f * Mathf.Sqrt(2f), 0f), Vector2.one, 45f), true);
            AssertSymmetric(diamond, Box(new Vector2(2f, 2f), Vector2.one, 45f), false);
        }

        [Test]
        public void NarrowParallelBoxesSeparateAndCrossingBoxesOverlap()
        {
            Vector2 forward = new Vector2(1f, 1f).normalized;
            Vector2 left = new(-forward.y, forward.x);
            Vector2 halfExtents = new(2f, 0.01f);
            CombatShapeData first = Box(Vector2.zero, halfExtents, 45f);

            AssertSymmetric(first, Box(left * 0.02f, halfExtents, 45f), true);
            AssertSymmetric(first, Box(left * 0.021f, halfExtents, 45f), false);
            AssertSymmetric(first, Box(Vector2.zero, halfExtents, -45f), true);
        }

        [Test]
        public void TinyShapesDoNotReceiveAWorldUnitToleranceFloor()
        {
            AssertSymmetric(Circle(Vector2.zero, 0.000001f), Circle(new Vector2(0.0000021f, 0f), 0.000001f), false);
            AssertSymmetric(Box(Vector2.zero, new Vector2(0.000001f, 0.000001f), 0f),
                Box(new Vector2(0.0000021f, 0f), new Vector2(0.000001f, 0.000001f), 0f), false);
        }

        [Test]
        public void BothBoxesAxesAreNeededToFindASeparatingAxis()
        {
            AssertSymmetric(
                Box(Vector2.zero, Vector2.one, 0f),
                Box(new Vector2(1.5f, 1.5f), new Vector2(0.1f, 2f), 45f),
                false);
        }

        [Test]
        public void ToWorldNormalizesDiagonalAndRotatesForwardLeftOffset()
        {
            var settings = new CombatShapeSettings(CombatShapeType.Box, new Vector2(2f, 1f), 0f, new Vector2(4f, 2f), 30f);
            CombatShapeData shape = CombatGeometryUtility.CreateWorldShape(settings, new Vector2(3f, -2f), new Vector2(7f, 7f));

            Assert.That(shape.Center.x, Is.EqualTo(3f + 1f / Mathf.Sqrt(2f)).Within(0.00001f));
            Assert.That(shape.Center.y, Is.EqualTo(-2f + 3f / Mathf.Sqrt(2f)).Within(0.00001f));
            Assert.That(shape.HalfExtents, Is.EqualTo(new Vector2(2f, 1f)));
            Assert.That(shape.RotationDegrees, Is.EqualTo(75f).Within(0.00001f));
        }

        [Test]
        public void LocalPositiveYIsLeftOfForward()
        {
            var settings = new CombatShapeSettings(CombatShapeType.Circle, Vector2.up, 0.25f, Vector2.zero, 0f);
            CombatShapeData shape = CombatGeometryUtility.CreateWorldShape(settings, Vector2.zero, new Vector2(0f, 20f));

            Assert.That(shape.Center, Is.EqualTo(Vector2.left));
            Assert.That(shape.Radius, Is.EqualTo(0.25f));
        }

        [Test]
        public void VerySmallAndLargeFiniteForwardsNormalizeWithoutUnderflowOrOverflow()
        {
            var settings = new CombatShapeSettings(CombatShapeType.Circle, Vector2.right, 1f, Vector2.zero, 0f);
            CombatShapeData tiny = CombatGeometryUtility.CreateWorldShape(settings, Vector2.zero, new Vector2(float.Epsilon, 0f));
            CombatShapeData large = CombatGeometryUtility.CreateWorldShape(settings, Vector2.zero, new Vector2(float.MaxValue, 0f));

            Assert.That(tiny.Center, Is.EqualTo(Vector2.right));
            Assert.That(large.Center, Is.EqualTo(Vector2.right));
        }

        [Test]
        public void ProjectionToXYIgnoresWorldZ()
        {
            var settings = new CombatShapeSettings(CombatShapeType.Circle, Vector2.zero, 1f, Vector2.zero, 0f);
            CombatShapeData first = CombatGeometryUtility.CreateWorldShape(settings, new Vector3(2f, 3f, -100f), Vector2.right);
            CombatShapeData second = CombatGeometryUtility.CreateWorldShape(settings, new Vector3(2f, 3f, 100f), Vector2.right);

            Assert.That(first.Center, Is.EqualTo(second.Center));
            Assert.That(CombatGeometryUtility.Overlaps(first, second), Is.True);
        }

        [TestCase(0f)]
        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void NonpositiveOrNonfiniteCircleRadiusIsRejected(float radius)
        {
            var settings = new CombatShapeSettings(CombatShapeType.Circle, Vector2.zero, radius, Vector2.zero, 0f);
            Assert.Throws<ArgumentException>(() => CombatGeometryUtility.CreateWorldShape(settings, Vector2.zero, Vector2.right));
            Assert.Throws<ArgumentException>(() => CombatGeometryUtility.Overlaps(Circle(Vector2.zero, radius), Circle(Vector2.zero, 1f)));
        }

        [TestCase(0f, 1f)]
        [TestCase(1f, 0f)]
        [TestCase(-1f, 1f)]
        [TestCase(1f, float.NaN)]
        [TestCase(float.PositiveInfinity, 1f)]
        public void InvalidBoxDimensionsAreRejected(float x, float y)
        {
            var settings = new CombatShapeSettings(CombatShapeType.Box, Vector2.zero, 0f, new Vector2(x, y), 0f);
            Assert.Throws<ArgumentException>(() => CombatGeometryUtility.CreateWorldShape(settings, Vector2.zero, Vector2.right));
            Assert.Throws<ArgumentException>(() => CombatGeometryUtility.Overlaps(Box(Vector2.zero, new Vector2(x, y), 0f), Circle(Vector2.zero, 1f)));
        }

        [Test]
        public void NonfinitePoseAndUnusedAuthoredFieldsAreRejected()
        {
            var settings = new CombatShapeSettings(CombatShapeType.Circle, Vector2.zero, 1f, Vector2.zero, 0f);
            Assert.Throws<ArgumentException>(() => CombatGeometryUtility.CreateWorldShape(settings, new Vector2(float.NaN, 0f), Vector2.right));
            Assert.Throws<ArgumentException>(() => CombatGeometryUtility.CreateWorldShape(settings, Vector2.zero, Vector2.zero));
            Assert.Throws<ArgumentException>(() => CombatGeometryUtility.CreateWorldShape(settings, Vector2.zero, new Vector2(float.PositiveInfinity, 0f)));
            Assert.Throws<ArgumentException>(() => CombatGeometryUtility.CreateWorldShape(
                new CombatShapeSettings(CombatShapeType.Circle, Vector2.zero, 1f, new Vector2(float.NaN, 0f), 0f), Vector2.zero, Vector2.right));
            Assert.Throws<ArgumentException>(() => CombatGeometryUtility.CreateWorldShape(
                new CombatShapeSettings(CombatShapeType.Circle, new Vector2(0f, float.NaN), 1f, Vector2.zero, 0f), Vector2.zero, Vector2.right));
            Assert.Throws<ArgumentException>(() => CombatGeometryUtility.Overlaps(
                Box(Vector2.zero, Vector2.one, float.NaN), Circle(Vector2.zero, 1f)));
            Assert.Throws<ArgumentException>(() => CombatGeometryUtility.Overlaps(
                Circle(new Vector2(float.PositiveInfinity, 0f), 1f), Circle(Vector2.zero, 1f)));
        }

        [Test]
        public void UnsupportedShapeTypeIsRejected()
        {
            var settings = new CombatShapeSettings((CombatShapeType)255, Vector2.zero, 1f, Vector2.one, 0f);
            Assert.Throws<ArgumentException>(() => CombatGeometryUtility.CreateWorldShape(settings, Vector2.zero, Vector2.right));
            Assert.Throws<ArgumentException>(() => CombatGeometryUtility.Overlaps(
                new CombatShapeData((CombatShapeType)255, Vector2.zero, Vector2.one, 1f, 0f), Circle(Vector2.zero, 1f)));
        }

        private static CombatShapeData Circle(Vector2 center, float radius)
        {
            return new CombatShapeData(CombatShapeType.Circle, center, Vector2.zero, radius, 0f);
        }

        private static CombatShapeData Box(Vector2 center, Vector2 halfExtents, float rotation)
        {
            return new CombatShapeData(CombatShapeType.Box, center, halfExtents, 0f, rotation);
        }

        private static void AssertSymmetric(CombatShapeData first, CombatShapeData second, bool expected)
        {
            Assert.That(CombatGeometryUtility.Overlaps(first, second), Is.EqualTo(expected));
            Assert.That(CombatGeometryUtility.Overlaps(second, first), Is.EqualTo(expected));
        }
    }
}
