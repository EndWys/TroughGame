using UnityEngine;
using ProjectCore.GameCore;
using Zenject;

namespace ProjectCore.Prototype
{
    public sealed class ClimbDetectorComponent : BaseNetworkEntityComponent
    {
        [Header("Climb Check Settings")]
        [SerializeField] private Transform _climbCheckPivot;
        [SerializeField] private LayerMask _climbableLayer;
        [SerializeField] private float _checkRadius = 0.3f;
        [SerializeField] private float _checkDistance = 0.5f;
        
        private IClimbDetectorDataMutator _climbDetectorDataMutator;

        [Inject]
        private void Constructor(IClimbDetectorDataMutator climbDetectorDataMutator)
        {
            _climbDetectorDataMutator = climbDetectorDataMutator;
        }
        
        public void PerformWallCheck()
        {
            Vector3 direction = transform.forward;
            
            bool hit = Physics.SphereCast(
                _climbCheckPivot.position,
                _checkRadius,
                direction,
                out RaycastHit hitInfo,
                _checkDistance,
                _climbableLayer
            );

            _climbDetectorDataMutator.ChangeWallAvailability(hit);
            _climbDetectorDataMutator.ChangeCurrentWallNormal(hit ? hitInfo.normal : Vector3.zero);
        }

        public override void NetworkTick()
        {
            if (!ShouldPerformCheck())
            {
                return;
            }

            PerformWallCheck();
        }

        private bool ShouldPerformCheck()
        {
            return ParentNetworkBehaviour.HasStateAuthority || ParentNetworkBehaviour.HasInputAuthority;
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (_climbCheckPivot == null) return;

            Vector3 origin = _climbCheckPivot.position;
            Vector3 direction = transform.forward;
            Vector3 maxDistanceCenter = origin + direction * _checkDistance;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, _checkRadius);
            
            bool isHit = Physics.SphereCast(
                origin, 
                _checkRadius, 
                direction, 
                out RaycastHit hitInfo, 
                _checkDistance, 
                _climbableLayer
            );

            if (isHit)
            {
                Gizmos.color = Color.green;
                Vector3 hitCenter = origin + direction * hitInfo.distance;
                Gizmos.DrawWireSphere(hitCenter, _checkRadius);
                
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(hitInfo.point, hitInfo.normal * 0.5f);
                
                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, hitCenter);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(maxDistanceCenter, _checkRadius);
                
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, maxDistanceCenter);
            }
        }

#endif
    }
}
