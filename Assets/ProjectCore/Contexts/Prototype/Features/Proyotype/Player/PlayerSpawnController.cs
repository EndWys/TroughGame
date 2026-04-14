using Domain;
using Fusion;
using UnityEngine;

namespace Prototype.Prototype
{
    public class PlayerSpawnController : BaseNetworkCallbacksBehaviour
    {
        [SerializeField] private NetworkPrefabRef _playerPrefab;

        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (HasStateAuthority)
            {
                runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, player);
            }
        }
    }
}