using Domain;
using Fusion;
using UnityEngine;

namespace ProjectCore.Features.Prototype.Player
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