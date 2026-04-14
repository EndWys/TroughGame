using System.Collections.Generic;
using Fusion;
using ProjectCore.Features.Prototype.Player.PlayerMediator;
using UnityEngine;

namespace ProjectCore.Features.Proyotype.Player.PlayerMediator.Colleagues
{
    public class PoseController : NetworkBehaviour
    {
        [SerializeField] private CapsuleCollider _playerCollider;
        [Space]
        [SerializeField] private LayerMask _obstaclesLayer;
        
        [Header("POSES HIGH")]
        [SerializeField] private float _crouchHeightMultiplier = 0.6f;
        
        [Header("NETWORKED DATA")]
        [UnitySerializeField, Networked] private PoseTypes CurrentPose { get; set; }

        private Dictionary<PoseTypes, float> _posesHigh;
        
        private float _originalHigh;
        
        public override void Spawned()
        {
            CurrentPose = PoseTypes.Stand;
            _originalHigh = _playerCollider.height;

            _posesHigh = new Dictionary<PoseTypes, float>()
            {
                { PoseTypes.Stand , _originalHigh },
                { PoseTypes.Crouch , _originalHigh * _crouchHeightMultiplier }, 
            };
            
            base.Spawned();
        }
        
        public bool CanChangePose(PoseTypes newPose)
        {
            if (!_posesHigh.TryGetValue(newPose, out float targetHeight))
            {
                return false;
            }
            
            if (targetHeight <= _playerCollider.height)
            {
                return true;
            }

            float radius = _playerCollider.radius * 0.9f;
            float heightDifference = targetHeight - _playerCollider.height;
            
            return !Physics.SphereCast(GetCastSphereOrigin(radius), radius, transform.up, out _, heightDifference, _obstaclesLayer);
        }

        public void SetPose(PoseTypes newPose)
        {
            if (CurrentPose == newPose)
            {
                return;
            }
            
            if (!_posesHigh.TryGetValue(newPose, out float targetHeight))
            {
                return;
            }
            
            CurrentPose = newPose;
            
            float bottomYOffset = _playerCollider.center.y - (_playerCollider.height / 2f);
            
            _playerCollider.height = targetHeight;
            
            Vector3 newCenter = _playerCollider.center;
            newCenter.y = bottomYOffset + (targetHeight / 2f);
            _playerCollider.center = newCenter;
        }
        
        private Vector3 GetCastSphereOrigin(float radius)
        {
            Vector3 colliderPosition = _playerCollider.transform.position;
            Vector3 colliderCenter = _playerCollider.center;
            Vector3 up = _playerCollider.transform.up;
            
            return  colliderPosition + colliderCenter + up * (_playerCollider.height / 2f - radius);
        }
    }
}