using Domain;
using Fusion;
using UnityEngine;

namespace Prototype.Prototype
{
    public readonly struct PlayerSpawnPayload : INetworkEntitySpawnPayload
    {
        public PlayerSpawnPayload(
            NetworkRunner runner,
            NetworkPrefabRef prefab,
            PlayerRef inputAuthority,
            Vector3 position,
            Quaternion rotation)
        {
            Runner = runner;
            Prefab = prefab;
            InputAuthority = inputAuthority;
            Position = position;
            Rotation = rotation;
        }

        public NetworkRunner Runner { get; }
        public NetworkPrefabRef Prefab { get; }
        public PlayerRef InputAuthority { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
    }
}
