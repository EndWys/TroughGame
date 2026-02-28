using Fusion;
using UnityEngine;

namespace ProjectCore.Features.Proyotype.Player
{
    public class GroundChecker : NetworkBehaviour
    {
        [Header("Ground Check Settings")]
        [SerializeField] private LayerMask _groundLayer;
        [SerializeField] private float _groundCheckRadius = 0.3f;
        [SerializeField] private float _groundCheckDistance = 0.2f;
        [SerializeField] private Vector3 _sphereCastOffset = new Vector3(0, -0.5f, 0);

        [Header("NETWORKED DATA")]
        [UnitySerializeField][Networked] public bool IsGrounded { get; private set; }
        [UnitySerializeField][Networked] public Vector3 GroundNormal { get; private set; }

        private ChangeDetector _changeDetector;
        
        public override void Spawned()
        {
            Runner.SetIsSimulated(Object, true);
            
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }
        
        public override void FixedUpdateNetwork()
        {
            PerformGroundCheck();

            TryToDetectChanges();
        }

        private void PerformGroundCheck()
        {
            Vector3 origin = transform.position + _sphereCastOffset;
    
            bool hit = Physics.SphereCast(
                origin, 
                _groundCheckRadius, 
                Vector3.down, 
                out RaycastHit hitInfo, 
                _groundCheckDistance, 
                _groundLayer
            );

            IsGrounded = hit;
            GroundNormal = hit ? hitInfo.normal : Vector3.up;
        }

        private void TryToDetectChanges()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(IsGrounded):
                        Debug.Log("IsGrounded: " + IsGrounded);
                        break;
                    case nameof(GroundNormal):
                        Debug.Log("GroundNormal: " + GroundNormal);
                        break;
                }
            }
        }
    }
}