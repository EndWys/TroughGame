using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public readonly struct PlayerEntityPayload : ILocomotionPayload
    {
        public PlayerEntityPayload(Vector2 direction)
        {
            Direction = direction;
        }

        public Vector2 Direction { get; }
    }
}
