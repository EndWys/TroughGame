using Fusion;
using ProjectCore.Domain.Scripts.NetworkUtilities;
using UnityEngine;

namespace ProjectCore.Features.Proyotype.Player
{
    public class PlayerSpawnController : BaseNetworkCallbacksBehaviour
    {
        [SerializeField] private NetworkPrefabRef _playerPrefab;

        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (runner.LocalPlayer == player)
            {
                runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, player);
            }
        }
    }
}