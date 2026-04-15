using System.Collections.Generic;
using Domain;
using Fusion;
using UnityEngine;

namespace Prototype.Prototype
{
    public class PlayerMovement : BaseNetworkStateMachine<EMovementState, BaseMovementState, PlayerInputData>, IPlayerColleague
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
        
        [UnitySerializeField, Networked] public override EMovementState CurrentState { get; protected set; }
        [UnitySerializeField, Networked] public override EMovementState PreviousState { get; protected set; }
        
        private IMediator<IPlayerColleague, EPlayerEventType> _mediator;
        
        public override void Spawned()
        {
            base.Spawned();
            
            ChangeState(EMovementState.Idle);
        }

        public override Dictionary<EMovementState, BaseMovementState> CreateStatesDictionary()
        {
            return new Dictionary<EMovementState, BaseMovementState>
            {
                { EMovementState.Default, new DefaultMovementState(this) },
                { EMovementState.Idle, new IdleMovementState(this) },
                { EMovementState.Walk, new LocomotionMovementState(this) },
                { EMovementState.Run, new LocomotionMovementState(this) },
                { EMovementState.Airborne, new AirborneMovementState(this) },
                { EMovementState.Crouch, new CrouchMovementState(this) },
                { EMovementState.Climb, new ClimbMovementState(this) }
            };
        }

        public void Init(IMediator<IPlayerColleague, EPlayerEventType> mediator)
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
                
            UpdateStates(input);
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
        }
        
        protected override void BeforePreviousStateExit()
        {
            _mediator.Notify(this, EPlayerEventType.OnPlayerMovementStateChange,
                new MovementStateChangedPayload() 
                {
                    MovementState = CurrentState,
                    PreviousMovementState = PreviousState,
                });
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
    }
}