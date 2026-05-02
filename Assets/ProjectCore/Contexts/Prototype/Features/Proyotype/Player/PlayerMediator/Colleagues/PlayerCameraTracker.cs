using Domain;
using UnityEngine;
using Zenject;

namespace Prototype.Prototype
{
    public class PlayerCameraTracker : 
        BaseNetworkEntityComponent, 
        IPlayerColleague
    {
        [Header("REFERENCES")]
        [SerializeField] private Transform _playerTarget;
        [SerializeField] private Transform _cameraLookTarget;
     
        [Header("FOV SETTINGS")]
        [SerializeField] private float _baseFOV = 60f;
        [SerializeField] private float _bustedFOVBonus = 10f;
        
        private INetworkBehaviourAccessor _networkBehaviourAccessor;
        private IMediator<IPlayerColleague, PlayerEventTypes> _mediator;
        private FollowCameraController _cameraController;

        [Inject]
        private void Construct(INetworkBehaviourAccessor networkBehaviourAccessor)
        {
            _networkBehaviourAccessor = networkBehaviourAccessor;
        }
        
        protected override void Init()
        {
            if (!_networkBehaviourAccessor.ParentNetworkBehaviour.HasInputAuthority)
            {
                return;
            }
            
            _cameraController = FindAnyObjectByType<FollowCameraController>();
            
            if (_cameraController != null)
            {
                _cameraController.SetTarget(_playerTarget, _cameraLookTarget);
            }
        }
        
        public void SetMediator(IMediator<IPlayerColleague, PlayerEventTypes> mediator)
        {
            _mediator = mediator;
        }

        public void ChangeFieldOfView(MovementStateChangedPayload movementStateChangedPayload)
        {
            if (_cameraController == null)
            {
                return;
            }
            
            _cameraController.ChangeFov(movementStateChangedPayload.MovementStates == MovementStates.Run ? _baseFOV + _bustedFOVBonus : _baseFOV);
        }
    }
}
