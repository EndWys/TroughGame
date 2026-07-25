using Fusion;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.Prototype
{
    public sealed class PlayerSpawnComponent : BaseNetworkCallbacksBehaviour
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
