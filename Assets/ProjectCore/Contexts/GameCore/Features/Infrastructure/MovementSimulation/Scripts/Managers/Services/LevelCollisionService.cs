using System;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class LevelCollisionService : IMovementCollisionStrategy
    {
        private const float SkinWidth = 0.01f;
        private const float MinimumDisplacement = 0.0001f;
        private const float SurfaceDirectionTolerance = 0.0001f;
        private const int MaximumDepenetrationIterations = 16;
        private const int MaximumClearanceSearchIterations = 16;
        private const int MaximumSlideIterations = 4;
        private const int MaximumCastHits = 16;
        private const int MaximumOverlapColliders = 16;

        private readonly RaycastHit2D[] _castHits = new RaycastHit2D[MaximumCastHits];
        private readonly Collider2D[] _overlapColliders =
            new Collider2D[MaximumOverlapColliders];

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

            resolvedDisplacement += ResolveInitialOverlap(
                ref currentPosition,
                radius,
                desiredDisplacement,
                contactFilter);

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

                if (!TryFindClosestBlockingHit(
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
                    resolvedDisplacement += displacementAfterTravel;
                    break;
                }

                remainingDisplacement = displacementAfterTravel -
                                        surfaceNormal * displacementIntoSurface;
            }

            return resolvedDisplacement;
        }

        private Vector2 ResolveInitialOverlap(
            ref Vector2 currentPosition,
            float radius,
            Vector2 desiredDisplacement,
            ContactFilter2D contactFilter)
        {
            Vector2 correction = Vector2.zero;
            Vector2 probeDirection = desiredDisplacement.sqrMagnitude >
                                     MinimumDisplacement * MinimumDisplacement
                ? desiredDisplacement.normalized
                : Vector2.right;

            for (int iteration = 0;
                 iteration < MaximumDepenetrationIterations;
                 iteration++)
            {
                int overlapCount = Overlap(
                    currentPosition,
                    radius,
                    contactFilter);

                if (overlapCount == 0)
                {
                    break;
                }

                if (!TryCreateOverlapCorrection(
                        currentPosition,
                        radius,
                        probeDirection,
                        contactFilter,
                        overlapCount,
                        out Vector2 iterationCorrection))
                {
                    break;
                }

                currentPosition += iterationCorrection;
                correction += iterationCorrection;
                probeDirection = iterationCorrection.normalized;
            }

            return correction;
        }

        private bool TryCreateOverlapCorrection(
            Vector2 position,
            float radius,
            Vector2 probeDirection,
            ContactFilter2D contactFilter,
            int overlapCount,
            out Vector2 correction)
        {
            Collider2D closestCollider = null;
            Vector2 surfaceNormal = Vector2.zero;
            float closestSurfaceDistance = float.PositiveInfinity;

            for (int index = 0; index < overlapCount; index++)
            {
                Collider2D overlapCollider = _overlapColliders[index];
                if (overlapCollider == null)
                {
                    continue;
                }

                Vector2 closestPoint = overlapCollider.ClosestPoint(position);
                Vector2 separation = position - closestPoint;
                float surfaceDistance = separation.magnitude;

                if (surfaceDistance <= MinimumDisplacement ||
                    surfaceDistance >= closestSurfaceDistance)
                {
                    continue;
                }

                closestCollider = overlapCollider;
                surfaceNormal = separation / surfaceDistance;
                closestSurfaceDistance = surfaceDistance;
            }

            if (closestCollider != null)
            {
                float correctionDistance = Mathf.Max(
                    SkinWidth,
                    radius - closestSurfaceDistance + SkinWidth);
                correction = surfaceNormal * correctionDistance;
                return true;
            }

            if (!TryFindClosestHit(
                    position,
                    radius,
                    probeDirection,
                    SkinWidth,
                    contactFilter,
                    out RaycastHit2D overlapHit))
            {
                correction = Vector2.zero;
                return false;
            }

            surfaceNormal = overlapHit.normal.normalized;
            if (surfaceNormal.sqrMagnitude <= Mathf.Epsilon)
            {
                correction = Vector2.zero;
                return false;
            }

            float clearanceDistance = FindClearanceDistance(
                position,
                radius,
                surfaceNormal,
                overlapHit.collider,
                contactFilter);

            if (clearanceDistance <= MinimumDisplacement)
            {
                correction = Vector2.zero;
                return false;
            }

            correction = surfaceNormal * (clearanceDistance + SkinWidth);
            return true;
        }

        private float FindClearanceDistance(
            Vector2 position,
            float radius,
            Vector2 direction,
            Collider2D targetCollider,
            ContactFilter2D contactFilter)
        {
            float maximumDistance =
                targetCollider.bounds.size.magnitude + radius + SkinWidth;
            float blockedDistance = 0f;
            float clearDistance = Mathf.Min(
                Mathf.Max(radius, SkinWidth),
                maximumDistance);

            while (clearDistance < maximumDistance &&
                   IsOverlapping(
                       position + direction * clearDistance,
                       radius,
                       targetCollider,
                       contactFilter))
            {
                blockedDistance = clearDistance;
                clearDistance = Mathf.Min(
                    clearDistance * 2f,
                    maximumDistance);
            }

            if (IsOverlapping(
                    position + direction * clearDistance,
                    radius,
                    targetCollider,
                    contactFilter))
            {
                return 0f;
            }

            for (int iteration = 0;
                 iteration < MaximumClearanceSearchIterations;
                 iteration++)
            {
                float candidateDistance =
                    (blockedDistance + clearDistance) * 0.5f;

                if (IsOverlapping(
                        position + direction * candidateDistance,
                        radius,
                        targetCollider,
                        contactFilter))
                {
                    blockedDistance = candidateDistance;
                }
                else
                {
                    clearDistance = candidateDistance;
                }
            }

            return clearDistance;
        }

        private bool IsOverlapping(
            Vector2 position,
            float radius,
            Collider2D targetCollider,
            ContactFilter2D contactFilter)
        {
            int overlapCount = Overlap(position, radius, contactFilter);

            for (int index = 0; index < overlapCount; index++)
            {
                if (_overlapColliders[index] == targetCollider)
                {
                    return true;
                }
            }

            return false;
        }

        private int Overlap(
            Vector2 position,
            float radius,
            ContactFilter2D contactFilter)
        {
            return Physics2D.OverlapCircle(
                position,
                radius,
                contactFilter,
                _overlapColliders);
        }

        private bool TryFindClosestBlockingHit(
            Vector2 origin,
            float radius,
            Vector2 direction,
            float distance,
            ContactFilter2D contactFilter,
            out RaycastHit2D closestHit)
        {
            int hitCount = Cast(
                origin,
                radius,
                direction,
                distance,
                contactFilter);

            closestHit = default;
            float closestDistance = float.PositiveInfinity;

            for (int index = 0; index < hitCount; index++)
            {
                RaycastHit2D hit = _castHits[index];
                if (hit.collider == null ||
                    hit.distance >= closestDistance ||
                    Vector2.Dot(direction, hit.normal) >=
                    -SurfaceDirectionTolerance)
                {
                    continue;
                }

                closestHit = hit;
                closestDistance = hit.distance;
            }

            return closestHit.collider != null;
        }

        private bool TryFindClosestHit(
            Vector2 origin,
            float radius,
            Vector2 direction,
            float distance,
            ContactFilter2D contactFilter,
            out RaycastHit2D closestHit)
        {
            int hitCount = Cast(
                origin,
                radius,
                direction,
                distance,
                contactFilter);

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

        private int Cast(
            Vector2 origin,
            float radius,
            Vector2 direction,
            float distance,
            ContactFilter2D contactFilter)
        {
            return Physics2D.CircleCast(
                origin,
                radius,
                direction,
                contactFilter,
                _castHits,
                distance);
        }
    }
}
