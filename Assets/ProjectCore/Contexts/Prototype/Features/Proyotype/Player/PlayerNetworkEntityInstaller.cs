using UnityEngine;

namespace Prototype.Prototype
{
    public class PlayerNetworkEntityInstaller : BaseNetworkEntityInstaller<PlayerNetworkEntity>
    {
        [SerializeField] private GroundChecker _groundChecker;
        [SerializeField] private PoseController _poseController;
        [SerializeField] private ClimbingChecker _climbingChecker;
        
        protected override void BindAdditionalComponents()
        {
            BindComponentFromInstance(_climbingChecker);
            BindComponentFromInstance(_poseController);
            BindComponentFromInstance(_groundChecker);
        }
    }
}