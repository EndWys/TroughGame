using Fusion;
using ProjectCore.Domain.Scripts.Paterns.Mediator;
using ProjectCore.Features.Prototype.Player.PlayerMediator;
using ProjectCore.Features.Prototype.Player.PlayerMediator.EventPayloads;
using ProjectCore.Features.Proyotype.Player;
using UnityEngine;

namespace ProjectCore.Features.Prototype.Player
{
    public class PlayerMovement : NetworkBehaviour, IPlayerColleague
    {
        [Header("REFERENCES")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private GroundChecker _groundChecker;
        
        [Header("SETTINGS")]
        [SerializeField] private float _walkSpeed = 5f;
        [SerializeField] private float _runSpeed = 8f;
        [SerializeField] private float _rotationSpeed = 150f;
        [SerializeField] private float _airControlMultiplier = 0.5f;
        
#if UNITY_EDITOR
        [Header("DEBUG")]
        [SerializeField] private bool _drawMovementGizmos = true;
        [SerializeField] private Vector3 _debugBaseDirection;
        [SerializeField] private Vector3 _debugProjectedDirection;
#endif
        
        [Header("NETWORKED DATA")]
        [UnitySerializeField][Networked] private EMovementState MovementState { get; set; } = EMovementState.Idle;
        [UnitySerializeField][Networked] private EMovementState PreviousMovementState { get; set; } = EMovementState.Idle;
        
        
        private IMediator<IPlayerColleague, EPlayerEventType> _mediator;
        private ChangeDetector _changeDetector;
        
        public override void Spawned()
        {
            Runner.SetIsSimulated(Object, true);
            
            _changeDetector = GetChangeDetector(ChangeDetector.Source.SimulationState);
        }
        
        public void Initialize(IMediator<IPlayerColleague, EPlayerEventType> mediator)
        {
            _mediator = mediator;
        }
        
        public override void FixedUpdateNetwork()
        {
            if (GetInput(out PlayerInputData input))
            {
                ProcessRotation(input.LookYawDelta);
                ProcessMovement(input);
            }

            TryToCheckChanges();
        }

        private void ProcessRotation(float yawDelta)
        {
            if (Mathf.Abs(yawDelta) > 0.01f)
            {
                float rotationStep = yawDelta * _rotationSpeed * Runner.DeltaTime;
                Quaternion deltaRotation = Quaternion.Euler(0, rotationStep, 0);
                _rigidbody.MoveRotation(_rigidbody.rotation * deltaRotation);
            }
        }

        private void ProcessMovement(PlayerInputData input)
        {
            Vector3 moveDirection = (transform.forward * input.MoveDirection.y + transform.right * input.MoveDirection.x).normalized;

#if UNITY_EDITOR
            _debugBaseDirection = moveDirection;
#endif
            
            if (moveDirection.sqrMagnitude > 0f)
            {
                float currentSpeed = input.IsRunning ? _runSpeed : _walkSpeed;
                
                Vector3 targetVelocity = GetTargetVelocity(moveDirection, currentSpeed);
                
                ApplyVelocityChange(targetVelocity);

                TryChangeMovementState(input.IsRunning ? EMovementState.Run : EMovementState.Walk);
            }
            else if (_groundChecker.IsGrounded)
            {
                ApplyVelocityChange(Vector3.zero);
                TryChangeMovementState(EMovementState.Idle);
            }
        }

        private Vector3 GetTargetVelocity(Vector3 movementDirection, float currentSpeed)
        {
            if (_groundChecker.IsGrounded)
            {
                Vector3 projectedDirection = Vector3.ProjectOnPlane(movementDirection, _groundChecker.GroundNormal).normalized;
                
#if UNITY_EDITOR
                _debugProjectedDirection = projectedDirection;
#endif
                
                return projectedDirection * currentSpeed;
            }
            
#if UNITY_EDITOR
            _debugProjectedDirection = Vector3.zero;
#endif

            return movementDirection * (currentSpeed * _airControlMultiplier);
        }
        
        private void ApplyVelocityChange(Vector3 targetVelocity)
        {
            Vector3 currentVelocity = _rigidbody.linearVelocity;
            
            Vector3 velocityChange = targetVelocity - currentVelocity;
            
            if (!_groundChecker.IsGrounded)
            {
                velocityChange.y = 0;
            }
            
            _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        private void TryChangeMovementState(EMovementState newState)
        {
            if (MovementState != newState)
            {
                PreviousMovementState = MovementState;
                MovementState = newState;
            }
        }

        private void TryToCheckChanges()
        {
            foreach (var change in _changeDetector.DetectChanges(this))
            {
                switch (change)
                {
                    case nameof(MovementState):
                        _mediator.Notify(this, EPlayerEventType.OnPlayerMovementStateChange,
                            new MovementStateChangedPayload() 
                            {
                                MovementState = MovementState,
                                PreviousMovementState = PreviousMovementState
                            });
                        break;
                }
            }
        }
        
#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_drawMovementGizmos)
            {
                return;
            }
            
            Vector3 startPos = transform.position + Vector3.up * 0.1f;
            
            if (_groundChecker != null && _groundChecker.Object != null && _groundChecker.Object.IsValid && _groundChecker.IsGrounded)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(startPos, _groundChecker.GroundNormal);
            }
            
            if (_debugBaseDirection.sqrMagnitude > 0)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(startPos, _debugBaseDirection);
            }
            
            if (_debugProjectedDirection.sqrMagnitude > 0)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(startPos, _debugProjectedDirection);
            }
            
            if (_rigidbody != null && _rigidbody.linearVelocity.sqrMagnitude > 0)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawRay(startPos, _rigidbody.linearVelocity.normalized);
            }
        }
#endif
    }
}