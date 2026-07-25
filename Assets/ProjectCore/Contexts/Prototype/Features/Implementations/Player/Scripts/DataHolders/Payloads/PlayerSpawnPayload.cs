using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.Prototype
{
    public readonly struct PlayerSpawnPayload : INetworkEntitySpawnPayload
    {
        public PlayerSpawnPayload(Vector3 position, Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
    }
}
