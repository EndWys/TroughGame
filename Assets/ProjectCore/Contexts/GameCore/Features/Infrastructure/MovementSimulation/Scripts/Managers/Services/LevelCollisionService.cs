using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class LevelCollisionService : IMovementCollisionStrategy
    {
        private const float SkinWidth = 0.01f;
        private const float MinimumDisplacement = 0.0001f;
        private const int MaximumSlideIterations = 4;
        private const int MaximumCastHits = 16;

        private readonly RaycastHit2D[] _castHits = new RaycastHit2D[MaximumCastHits];

        public Vector2 ResolveDisplacement(
            IMovementCollisionBodyAccessor body,
            Vector2 desiredDisplacement)
        {
            if (body == null)
            {
                throw new ArgumentNullException(nameof(body));
            }

            Vector2 currentPosition = body.Position;
            Vector2 remainingDisplacement = desiredDisplacement;
            Vector2 resolvedDisplacement = Vector2.zero;
            float radius = Mathf.Max(0f, body.CollisionRadius);

            ContactFilter2D contactFilter = new ContactFilter2D();
            contactFilter.SetLayerMask(body.CollisionMask);
            contactFilter.useTriggers = false;

            for (int iteration = 0;
                 iteration < MaximumSlideIterations;
                 iteration++)
            {
                float remainingDistance = remainingDisplacement.magnitude;

                if (remainingDistance <= MinimumDisplacement)
                {
                    break;
                }

                Vector2 direction = remainingDisplacement / remainingDistance;

                if (!TryFindClosestHit(
                        currentPosition,
                        radius,
                        direction,
                        remainingDistance + SkinWidth,
                        contactFilter,
                        out RaycastHit2D closestHit))
                {
                    resolvedDisplacement += remainingDisplacement;
                    break;
                }

                float travelDistance = Mathf.Clamp(
                    closestHit.distance - SkinWidth,
                    0f,
                    remainingDistance);
                Vector2 travelDisplacement = direction * travelDistance;

                currentPosition += travelDisplacement;
                resolvedDisplacement += travelDisplacement;

                Vector2 displacementAfterTravel =
                    remainingDisplacement - travelDisplacement;
                Vector2 surfaceNormal = closestHit.normal.normalized;

                if (surfaceNormal.sqrMagnitude <= Mathf.Epsilon)
                {
                    break;
                }

                float displacementIntoSurface = Vector2.Dot(
                    displacementAfterTravel,
                    surfaceNormal);

                if (displacementIntoSurface >= 0f)
                {
                    break;
                }

                remainingDisplacement = displacementAfterTravel -
                                        surfaceNormal * displacementIntoSurface;
            }

            return resolvedDisplacement;
        }

        private bool TryFindClosestHit(
            Vector2 origin,
            float radius,
            Vector2 direction,
            float distance,
            ContactFilter2D contactFilter,
            out RaycastHit2D closestHit)
        {
            int hitCount = Physics2D.CircleCast(
                origin,
                radius,
                direction,
                contactFilter,
                _castHits,
                distance);

            closestHit = default;
            float closestDistance = float.PositiveInfinity;

            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit2D hit = _castHits[index];

                if (hit.collider == null || hit.distance >= closestDistance)
                {
                    continue;
                }

                closestHit = hit;
                closestDistance = hit.distance;
            }

            return closestHit.collider != null;
        }
    }
}
