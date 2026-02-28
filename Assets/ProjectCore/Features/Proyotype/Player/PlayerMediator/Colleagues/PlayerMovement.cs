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

            if (moveDirection.sqrMagnitude > 0f)
            {
                float currentSpeed = input.IsRunning ? _runSpeed : _walkSpeed;
                Vector3 targetVelocity = moveDirection * currentSpeed;
                
                targetVelocity.y = _rigidbody.linearVelocity.y;
                _rigidbody.linearVelocity = targetVelocity;

                TryChangeMovementState(input.IsRunning ? EMovementState.Run : EMovementState.Walk);
            }
            else
            {
                _rigidbody.linearVelocity = new Vector3(0, _rigidbody.linearVelocity.y, 0);
                TryChangeMovementState(EMovementState.Idle);
            }
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
    }
}