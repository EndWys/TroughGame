using Domain;
using UnityEngine;

namespace Prototype.Prototype
{
    public class PlayerNetworkEntityInstaller : BaseNetworkEntityInstaller<PlayerNetworkEntity>
    {
        [SerializeField] private GroundDetectorComponent _groundDetectorComponent;
        [SerializeField] private PlayerPoseComponent _playerPoseComponent;
        [SerializeField] private ClimbDetectorComponent _climbDetectorComponent;
        [SerializeField] private PlayerMediator _playerMediator;

        protected override void BindAdditionalComponents()
        {
            Container.BindInterfacesAndSelfTo<PlayerMediator>()
                .FromInstance(_playerMediator)
                .AsCached();

            BindComponentFromInstance(_climbDetectorComponent);
            BindComponentFromInstance(_playerPoseComponent);
            BindComponentFromInstance(_groundDetectorComponent);
        }
    }
}
