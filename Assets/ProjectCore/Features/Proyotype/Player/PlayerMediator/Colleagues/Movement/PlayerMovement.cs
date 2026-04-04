using System.Collections.Generic;
using Fusion;
using ProjectCore.Domain.Scripts.Paterns.Mediator;
using ProjectCore.Features.Prototype.Player.Configs;
using ProjectCore.Features.Prototype.Player.PlayerMediator;
using ProjectCore.Features.Prototype.Player.PlayerMediator.EventPayloads;
using ProjectCore.Features.Proyotype.Player;
using UnityEngine;

namespace ProjectCore.Features.Prototype.Player
{
    public class PlayerMovement : NetworkBehaviour, IPlayerColleague
    {
        private Dictionary<EMovementState, BaseMovementState> _movementStates;
        
        private IMediator<IPlayerColleague, EPlayerEventType> _mediator;
        
        [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
        [field: SerializeField] public GroundChecker GroundChecker { get; private set; }
        
        [field:Space]
        [field:SerializeField] public LocomotionMovementConfig LocomotionConfig { get; private set; }
        [field:SerializeField] public AirborneMovementConfig AirborneConfig { get; private set; }

        [Header("NETWORKED DATA")]
        [UnitySerializeField][Networked] private EMovementState CurrentMovementState { get; set; }
        [UnitySerializeField][Networked] private EMovementState PreviousMovementState { get; set; }
        [field:Space]
        [UnitySerializeField][Networked] public TickTimer CoyoteTimer { get; private set; }
        [UnitySerializeField][Networked] public TickTimer JumpBufferTimer { get; private set; }
        [UnitySerializeField][Networked] public NetworkBool IsJumping { get; set; }
        
        
        public override void Spawned()
        {
            Runner.SetIsSimulated(Object, true);
            
            _movementStates = new Dictionary<EMovementState, BaseMovementState>
            {
                { EMovementState.Idle, new IdleMovementState(this) },
                { EMovementState.Walk, new LocomotionMovementState(this) },
                { EMovementState.Run, new LocomotionMovementState(this) },
                { EMovementState.Airborne, new AirborneMovementState(this) },
            };
            
            CurrentMovementState = EMovementState.Idle;
            _movementStates[CurrentMovementState].Enter();
        }
        
        public void Initialize(IMediator<IPlayerColleague, EPlayerEventType> mediator)
        {
            _mediator = mediator;
        }
        
        public override void FixedUpdateNetwork()
        {
            GroundChecker.PerformGroundCheck();
            
            if (!GetInput(out PlayerInputData input))
            {
                return;
            }
            
            ProcessRotation(input.LookYawDelta);
            
            if (GroundChecker.IsGrounded)
            {
                CoyoteTimer = TickTimer.CreateFromTicks(Runner, AirborneConfig.CoyoteTimeTicks);
                IsJumping = false;
            }
                
            if (input.IsJumpPressed)
            {
                Debug.Log("JUMP BUFFER");
                JumpBufferTimer = TickTimer.CreateFromTicks(Runner, AirborneConfig.JumpBufferTicks);
            }
                
            EMovementState nextState = _movementStates[CurrentMovementState].Tick(ref input);
                
            if (nextState == CurrentMovementState)
            {
                return;
            }
                
            ChangeState(nextState);
        }
        
        public void ExecuteJump()
        {
            Vector3 velocity = Rigidbody.linearVelocity;
            velocity.y = 0f;
            Rigidbody.linearVelocity = velocity;
    
            Rigidbody.AddForce(Vector3.up * AirborneConfig.JumpForce, ForceMode.Impulse);
            IsJumping = true;
            Debug.Log("JUMP");

            JumpBufferTimer = TickTimer.None;
            CoyoteTimer = TickTimer.None;
            
            _mediator.Notify(this, EPlayerEventType.OnPlayerJump, new JumpPayload());
        }
        
        public void ApplyVelocityChange(Vector3 targetVelocity)
        {
            Vector3 currentVelocity = Rigidbody.linearVelocity;
            
            Vector3 velocityChange = targetVelocity - currentVelocity;
            
            if (!GroundChecker.IsGrounded)
            {
                velocityChange.y = 0;
            }
            
            Rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        
        private void ProcessRotation(float yawDelta)
        {
            if (Mathf.Abs(yawDelta) > 0.01f)
            {
                float rotationStep = yawDelta * LocomotionConfig.RotationSpeed * Runner.DeltaTime;
                Quaternion deltaRotation = Quaternion.Euler(0, rotationStep, 0);
                Rigidbody.MoveRotation(Rigidbody.rotation * deltaRotation);
            }
        }
        
        private void ChangeState(EMovementState newState)
        {
            _movementStates[CurrentMovementState].Exit();
            
            PreviousMovementState = CurrentMovementState;
            CurrentMovementState = newState;
            
            _mediator.Notify(this, EPlayerEventType.OnPlayerMovementStateChange,
                new MovementStateChangedPayload() 
                {
                    PreviousMovementState = PreviousMovementState,
                    MovementState = CurrentMovementState,
                });
            
            _movementStates[CurrentMovementState].Enter();
        }
    }
}