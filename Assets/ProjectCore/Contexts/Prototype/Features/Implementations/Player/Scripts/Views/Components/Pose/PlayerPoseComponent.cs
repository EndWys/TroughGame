using System.Collections.Generic;
using ProjectCore.GameCore;
using Domain;
using UnityEngine;
using Zenject;

namespace ProjectCore.Prototype
{
    public class PlayerPoseComponent : BaseNetworkEntityComponent
    {
        [SerializeField] private CapsuleCollider _playerCollider;
        [Space]
        [SerializeField] private LayerMask _obstaclesLayer;
        
        [Header("POSES HIGH")]
        [SerializeField] private float _crouchHeightMultiplier = 0.6f;

        private IPoseDataMutator _poseDataMutator;
        
        private Dictionary<PoseTypes, float> _posesHigh;
        
        private float _originalHigh;

        [Inject]
        private void Construct(IPoseDataMutator poseDataMutator)
        {
            _poseDataMutator = poseDataMutator;
        }
        
        public override void Init()
        {
            if (!ShouldPerformPoseControl())
            {
                return;
            }

            _poseDataMutator.ChangePose(PoseTypes.Stand);
            _originalHigh = _playerCollider.height;

            _posesHigh = new Dictionary<PoseTypes, float>()
            {
                { PoseTypes.Stand , _originalHigh },
                { PoseTypes.Crouch , _originalHigh * _crouchHeightMultiplier }, 
            };
            
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
            if (_poseDataMutator.CurrentPose == newPose)
            {
                return;
            }
            
            if (!_posesHigh.TryGetValue(newPose, out float targetHeight))
            {
                return;
            }
            
            _poseDataMutator.ChangePose(newPose);
            
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

        private bool ShouldPerformPoseControl()
        {
            return ParentNetworkBehaviour.HasStateAuthority || ParentNetworkBehaviour.HasInputAuthority;
        }
    }
}
