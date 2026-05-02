using UnityEngine;
using Zenject;

namespace Prototype.Prototype
{
    public class GroundChecker : MonoBehaviour
    {
        [Header("Ground Check Settings")] 
        [SerializeField] private Transform _groundCheckPivot;
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _groundCheckRadius = 0.3f;
        [SerializeField] private float _groundCheckDistance = 0.2f;
        
        private IGroundDetectorDataChanger _groundDetectorDataChanger;

        [Inject]
        private void Constructor(IGroundDetectorDataChanger groundDetectorDataChanger)
        {
            _groundDetectorDataChanger = groundDetectorDataChanger;
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

            _groundDetectorDataChanger.ChangeGroundedStatus(hit);
            _groundDetectorDataChanger.ChangeGroundNormal(hit ? hitInfo.normal : Vector3.up);
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