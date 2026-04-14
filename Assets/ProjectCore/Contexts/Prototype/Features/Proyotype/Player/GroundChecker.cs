using Fusion;
using UnityEngine;

namespace ProjectCore.Features.Proyotype.Player
{
    public class GroundChecker : NetworkBehaviour
    {
        [Header("Ground Check Settings")] 
        [SerializeField] private Transform _groundCheckPivot;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _groundCheckRadius = 0.3f;
        [SerializeField] private float _groundCheckDistance = 0.2f;

        [Header("NETWORKED DATA")]
        [UnitySerializeField][Networked] public bool IsGrounded { get; private set; }
        [UnitySerializeField][Networked] public Vector3 GroundNormal { get; private set; }
        
        public override void Spawned()
        {
            Runner.SetIsSimulated(Object, true);
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

            IsGrounded = hit;
            GroundNormal = hit ? hitInfo.normal : Vector3.up;
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