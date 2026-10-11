using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public readonly struct PlayerInputFrameData
    {
        public PlayerInputFrameData(Vector2 direction)
        {
            Direction = direction;
        }

        public Vector2 Direction { get; }
    }
}
