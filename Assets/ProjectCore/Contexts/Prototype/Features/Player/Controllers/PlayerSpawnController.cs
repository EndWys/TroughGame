using Domain;
using Fusion;
using UnityEngine;
using Zenject;

namespace Prototype.Prototype
{
    public class PlayerSpawnController : BaseNetworkCallbacksBehaviour
    {
        [SerializeField] private NetworkPrefabRef _playerPrefab;
        
        [Inject]
        private void Construct()
        {
            
        }
        
        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (HasStateAuthority)
            {
                runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity, player);
            }
        }
    }
}