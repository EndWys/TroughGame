using Fusion;
using UnityEngine;

namespace Prototype.Prototype
{
    public class ClimbingChecker : NetworkBehaviour
    {
        [Header("Climb Check Settings")]
        [SerializeField] private Transform _climbCheckPivot;
        [SerializeField] private LayerMask _climbableLayer;
        [SerializeField] private float _checkRadius = 0.3f;
        [SerializeField] private float _checkDistance = 0.5f;
        
        [Header("NETWORKED DATA")]
        [UnitySerializeField][Networked] public NetworkBool NearValidWall { get; private set; }
        [UnitySerializeField][Networked] public Vector3 CurrentWallNormal { get; private set; }
        
        public override void Spawned()
        {
            Runner.SetIsSimulated(Object, true);
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

            NearValidWall = hit;
            CurrentWallNormal = hit ? hitInfo.normal : Vector3.zero;
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