using System;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public static class PlayerFacingDirectionUtility
    {
        public static PlayerFacingDirection FromVector(
            Vector2 direction,
            PlayerFacingDirection fallbackDirection,
            float axisThreshold)
        {
            Vector2 fallbackVector = ToVector(fallbackDirection);
            float horizontalDirection = Mathf.Abs(direction.x) > axisThreshold
                ? Mathf.Sign(direction.x)
                : fallbackVector.x;
            float verticalDirection = Mathf.Abs(direction.y) > axisThreshold
                ? Mathf.Sign(direction.y)
                : fallbackVector.y;

            if (verticalDirection > 0f)
            {
                return horizontalDirection > 0f
                    ? PlayerFacingDirection.NorthEast
                    : PlayerFacingDirection.NorthWest;
            }

            return horizontalDirection > 0f
                ? PlayerFacingDirection.SouthEast
                : PlayerFacingDirection.SouthWest;
        }

        public static Vector2 ToVector(PlayerFacingDirection facingDirection)
        {
            return facingDirection switch
            {
                PlayerFacingDirection.SouthEast => new Vector2(1f, -1f),
                PlayerFacingDirection.SouthWest => new Vector2(-1f, -1f),
                PlayerFacingDirection.NorthEast => new Vector2(1f, 1f),
                PlayerFacingDirection.NorthWest => new Vector2(-1f, 1f),
                _ => throw new ArgumentOutOfRangeException(nameof(facingDirection), facingDirection, null),
            };
        }
    }
}
