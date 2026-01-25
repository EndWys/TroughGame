using Fusion;
using ProjectCore.Domain.Scripts.Paterns.Mediator;
using ProjectCore.Features.Prototype.Player;
using ProjectCore.Features.Prototype.Player.PlayerMediator;
using UnityEngine;

namespace ProjectCore.Features.Proyotype.Player
{
    public class PlayerCameraTracker : NetworkBehaviour, IPlayerColleague
    {
        [SerializeField] private Transform _playerTarget;
        [SerializeField] private Transform _cameraLookTarget;
        
        private IMediator<IPlayerColleague, EPlayerEventType> _mediator;
        
        public void Initialize(IMediator<IPlayerColleague, EPlayerEventType> mediator)
        {
            _mediator = mediator;
        }
        
        public override void Spawned()
        {
            var cameraController = FindAnyObjectByType<FollowCameraController>();
            
            if (cameraController != null && Object.InputAuthority == Runner.LocalPlayer)
            {
                cameraController.SetTarget(_playerTarget, _cameraLookTarget);
            }
        }
    }
}
