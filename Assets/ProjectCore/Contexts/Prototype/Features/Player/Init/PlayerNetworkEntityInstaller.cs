using Domain;
using ProjectCore.GameCore;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectCore.Prototype
{
    public class PlayerNetworkEntityInstaller : BaseNetworkEntityInstaller<PlayerNetworkEntity>
    {
        [FormerlySerializedAs("_groundChecker")]
        [SerializeField] private GroundDetectorComponent _groundDetectorComponent;
        [FormerlySerializedAs("_poseController")]
        [SerializeField] private PlayerPoseComponent _playerPoseComponent;
        [FormerlySerializedAs("_climbingChecker")]
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
