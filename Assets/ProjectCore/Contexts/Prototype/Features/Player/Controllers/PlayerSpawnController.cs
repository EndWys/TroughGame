using Domain;
using Fusion;
using UnityEngine;
using Zenject;

namespace Prototype.Prototype
{
    public class PlayerSpawnController : BaseNetworkCallbacksBehaviour
    {
        [SerializeField] private NetworkPrefabRef _playerPrefab;

        private NetworkEntityIdFactory _networkEntityIdFactory;

        [Inject]
        private void Construct(NetworkEntityIdFactory networkEntityIdFactory)
        {
            _networkEntityIdFactory = networkEntityIdFactory;
        }

        public override void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
            if (HasStateAuthority)
            {
                NetworkEntityId entityId = _networkEntityIdFactory.Create(PlayerNetworkEntityTypes.Player);

                runner.Spawn(
                    _playerPrefab,
                    Vector3.zero,
                    Quaternion.identity,
                    player,
                    (_, networkObject) =>
                    {
                        if (networkObject.TryGetComponent(out PlayerNetworkEntity playerNetworkEntity))
                        {
                            playerNetworkEntity.SetEntityId(entityId);
                        }
                    });
            }
        }
    }
}
