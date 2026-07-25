using UnityEngine;
using ProjectCore.GameCore;
using Zenject;

namespace ProjectCore.Prototype
{
    public sealed class GroundDetectorComponent : BaseNetworkEntityComponent
    {
        [Header("Ground Check Settings")] 
        [SerializeField] private Transform _groundCheckPivot;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _groundCheckRadius = 0.3f;
        [SerializeField] private float _groundCheckDistance = 0.2f;
        
        private IGroundDetectorDataMutator _groundDetectorDataMutator;

        [Inject]
        private void Constructor(IGroundDetectorDataMutator groundDetectorDataMutator)
        {
            _groundDetectorDataMutator = groundDetectorDataMutator;
        }

        public void PerformGroundCheck()
        {
            bool hit = Physics.SphereCast(
                _groundCheckPivot.position, 
                _groundCheckRadius, 
                Vector3.down, 
                out RaycastHit hitInfo, 
                _groundCheckDistance, 
                _groundLayer
            );

            _groundDetectorDataMutator.ChangeGroundedStatus(hit);
            _groundDetectorDataMutator.ChangeGroundNormal(hit ? hitInfo.normal : Vector3.up);
        }

        public override void NetworkTick()
        {
            if (!ShouldPerformCheck())
            {
                return;
            }

            PerformGroundCheck();
        }

        private bool ShouldPerformCheck()
        {
            return ParentNetworkBehaviour.HasStateAuthority || ParentNetworkBehaviour.HasInputAuthority;
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (_groundCheckPivot == null) return;

            Vector3 origin = _groundCheckPivot.position;
            Vector3 maxDistanceCenter = origin + Vector3.down * _groundCheckDistance;
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin, _groundCheckRadius);
            
            bool isHit = Physics.SphereCast(
                origin, 
                _groundCheckRadius, 
                Vector3.down, 
                out RaycastHit hitInfo, 
                _groundCheckDistance, 
                _groundLayer
            );

            if (isHit)
            {
                Gizmos.color = Color.green;
                Vector3 hitCenter = origin + Vector3.down * hitInfo.distance;
                Gizmos.DrawWireSphere(hitCenter, _groundCheckRadius);
                
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(hitInfo.point, hitInfo.normal * 0.5f);
                
                Gizmos.color = Color.green;
                Gizmos.DrawLine(origin, hitCenter);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(maxDistanceCenter, _groundCheckRadius);
                
                Gizmos.color = Color.red;
                Gizmos.DrawLine(origin, maxDistanceCenter);
            }
        }

#endif
    }
}
