using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public readonly struct PlayerEntityPayload : ILocomotionPayload
    {
        public PlayerEntityPayload(PlayerInputSnapshotData input)
        {
            Input = input;
        }

        public PlayerInputSnapshotData Input { get; }
        Vector2 ILocomotionPayload.Direction => Input.MoveDirection;
    }
}
