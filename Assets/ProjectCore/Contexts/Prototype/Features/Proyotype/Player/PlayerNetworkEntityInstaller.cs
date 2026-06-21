using UnityEngine;

namespace Prototype.Prototype
{
    public class PlayerNetworkEntityInstaller : BaseNetworkEntityInstaller<PlayerNetworkEntity>
    {
        [SerializeField] private GroundChecker _groundChecker;
        [SerializeField] private PoseController _poseController;
        [SerializeField] private ClimbingChecker _climbingChecker;
        [SerializeField] private PlayerMediator _playerMediator;
        
        protected override void BindAdditionalComponents()
        {
            Container.BindInterfacesAndSelfTo<PlayerMediator>()
                .FromInstance(_playerMediator)
                .AsCached();

            BindComponentFromInstance(_climbingChecker);
            BindComponentFromInstance(_poseController);
            BindComponentFromInstance(_groundChecker);
        }
    }
}
