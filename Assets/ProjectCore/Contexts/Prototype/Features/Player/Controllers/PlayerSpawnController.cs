using Domain;
using Fusion;
using UnityEngine;
using Zenject;

namespace Prototype.Prototype
{
    public class PlayerSpawnController : BaseNetworkCallbacksBehaviour
    {
        [SerializeField] private NetworkPrefabRef _playerPrefab;

        private NetworkEntitySpawner _networkEntitySpawner;

        [Inject]
        private void Construct(NetworkEntitySpawner networkEntitySpawner)
        {
            _networkEntitySpawner = networkEntitySpawner;
        }

        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (HasStateAuthority)
            {
                _networkEntitySpawner.Spawn(
                    runner,
                    _playerPrefab,
                    new PlayerSpawnPayload(
                        Vector3.zero,
                        Quaternion.identity),
                    player);
            }
        }
    }
}
