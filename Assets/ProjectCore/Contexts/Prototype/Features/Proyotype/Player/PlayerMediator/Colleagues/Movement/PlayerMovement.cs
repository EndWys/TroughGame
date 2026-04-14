using System.Collections.Generic;
using Domain;
using Fusion;
using ProjectCore.Features.Prototype.Player.Configs;
using ProjectCore.Features.Prototype.Player.PlayerMediator;
using ProjectCore.Features.Prototype.Player.PlayerMediator.EventPayloads;
using ProjectCore.Features.Proyotype.Player;
using ProjectCore.Features.Proyotype.Player.PlayerMediator.Colleagues;
using UnityEngine;

namespace ProjectCore.Features.Prototype.Player
{
    public class PlayerMovement : NetworkBehaviour, IPlayerColleague
    {
        [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
        [field: SerializeField] public GroundChecker GroundChecker { get; private set; }
        [field: SerializeField] public PoseController PoseController { get; private set; }
        [field: SerializeField] public ClimbingChecker ClimbingChecker { get; private set; }
        
        [field:Space]
        [field:SerializeField] public LocomotionMovementConfig LocomotionConfig { get; private set; }
        [field:SerializeField] public AirborneMovementConfig AirborneConfig { get; private set; }
        [field:SerializeField] public CrouchMovementConfig CrouchConfig { get; private set; }

        [Header("NETWORKED DATA")]
        [UnitySerializeField, Networked] public TickTimer CoyoteTimer { get; private set; }
        [UnitySerializeField][Networked] public TickTimer JumpBufferTimer { get; private set; }
        [UnitySerializeField][Networked] public NetworkBool IsJumping { get; private set; }
        [field:Space]
        [UnitySerializeField][Networked] private EMovementState CurrentMovementState { get; set; }
        [UnitySerializeField][Networked] private EMovementState PreviousMovementState { get; set; }
        
        private Dictionary<EMovementState, BaseMovementState> _movementStates;
        
        private IMediator<IPlayerColleague, EPlayerEventType> _mediator;
        
        public override void Spawned()
        {
            Runner.SetIsSimulated(Object, true);
            
            _movementStates = new Dictionary<EMovementState, BaseMovementState>
            {
                { EMovementState.Idle, new IdleMovementState(this) },
                { EMovementState.Walk, new LocomotionMovementState(this) },
                { EMovementState.Run, new LocomotionMovementState(this) },
                { EMovementState.Airborne, new AirborneMovementState(this) },
                { EMovementState.Crouch, new CrouchMovementState(this) },
                { EMovementState.Climb, new ClimbMovementState(this) }
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
            ClimbingChecker.PerformWallCheck();
            
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
            
            ExecuteJump(Vector3.up);
        }
        
        public void ExecuteJump(Vector3 direction)
        {
            Rigidbody.AddForce(direction * AirborneConfig.JumpForce, ForceMode.Impulse);
            IsJumping = true;

            JumpBufferTimer = TickTimer.None;
            CoyoteTimer = TickTimer.None;
            
            _mediator.Notify(this, EPlayerEventType.OnPlayerJump, new JumpPayload());
            
            _mediator.Notify(this, EPlayerEventType.OnPlayerJump, new JumpPayload());
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