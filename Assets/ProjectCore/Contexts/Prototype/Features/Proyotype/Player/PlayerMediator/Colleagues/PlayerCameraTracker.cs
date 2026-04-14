using Domain;
using Fusion;
using UnityEngine;

namespace Prototype.Prototype
{
    public class PlayerCameraTracker : NetworkBehaviour, IPlayerColleague
    {
        [Header("REFERENCES")]
        [SerializeField] private Transform _playerTarget;
        [SerializeField] private Transform _cameraLookTarget;
     
        [Header("FOV SETTINGS")]
        [SerializeField] private float _baseFOV = 60f;
        [SerializeField] private float _bustedFOVBonus = 10f;
        
        private IMediator<IPlayerColleague, EPlayerEventType> _mediator;
        
        private FollowCameraController _cameraController;
        
        public void Init(IMediator<IPlayerColleague, EPlayerEventType> mediator)
        {
            _mediator = mediator;
        }
        
        public override void Spawned()
        {
            if (!HasInputAuthority)
            {
                return;
            }

            _cameraController = FindAnyObjectByType<FollowCameraController>();
            
            if (_cameraController != null)
            {
                _cameraController.SetTarget(_playerTarget, _cameraLookTarget);
            }
        }

        public void ChangeFieldOfView(MovementStateChangedPayload movementStateChangedPayload)
        {
            if (_cameraController == null)
            {
                return;
            }
            
            _cameraController.ChangeFov(movementStateChangedPayload.MovementState == EMovementState.Run ? _baseFOV + _bustedFOVBonus : _baseFOV);
        }
    }
}
