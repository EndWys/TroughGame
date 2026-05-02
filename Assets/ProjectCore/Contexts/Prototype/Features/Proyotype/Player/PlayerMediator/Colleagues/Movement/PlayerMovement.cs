using System.Collections.Generic;
using AYellowpaper.SerializedCollections;
using Domain;
using UnityEngine;
using Zenject;

namespace Prototype.Prototype
{
    public class PlayerMovement : 
        BaseNetworkEntityStateMachine<MovementStates, BasePlayerMovementState, PlayerInputData>, IPlayerColleague
    {
        [field: SerializeField] public Rigidbody Rigidbody { get; private set; }
        
        [field:Space]
        [field:SerializeField] public LocomotionMovementConfig LocomotionConfig { get; private set; }
        [field:SerializeField] public AirborneMovementConfig AirborneConfig { get; private set; }
        [field:SerializeField] public CrouchMovementConfig CrouchConfig { get; private set; }

        [Space]
        [SerializedDictionary("State Type", "State Object")]
        [SerializeField] private SerializedDictionary<MovementStates, BasePlayerMovementState> _movementStates;
        
        private GroundChecker _groundChecker;
        private PoseController _poseController;
        private ClimbingChecker _climbingChecker;
        
        private INetworkBehaviourAccessor _networkBehaviourAccessor;

        private IMediator<IPlayerColleague, PlayerEventTypes> _mediator;
        private IMovementStateDataChanger<MovementStates> _movementStateDataChanger;
        private IGroundDetectorDataChanger _groundDetectorDataChanger;
        private IJumpDataChanger _jumpDataChanger;

        [Inject]
        private void Construct(GroundChecker groundChecker,
            PoseController poseController,
            ClimbingChecker climbingChecker,
            INetworkBehaviourAccessor networkBehaviourAccessor,
            IMovementStateDataChanger<MovementStates> movementStateDataChanger,
            IJumpDataChanger jumpDataChanger,
            IGroundDetectorDataChanger groundDetectorDataChanger)
        {
            _groundChecker = groundChecker;
            _poseController = poseController;
            _climbingChecker = climbingChecker;
            
            _networkBehaviourAccessor = networkBehaviourAccessor;
            _movementStateDataChanger = movementStateDataChanger;
            _jumpDataChanger = jumpDataChanger;
            _groundDetectorDataChanger = groundDetectorDataChanger;
        }
        
        protected override void Init()
        {
            base.Init();

            foreach (var states in _movementStates.Values)
            {
                states.Init(this);
            }
            
            _poseController.Init();
            
            ChangeState(MovementStates.Idle);
        }

        public override Dictionary<MovementStates, BasePlayerMovementState> CreateStatesDictionary()
        {
            return _movementStates;
        }

        public void SetMediator(IMediator<IPlayerColleague, PlayerEventTypes> mediator)
        {
            _mediator = mediator;
        }

        public override void NetworkTick()
        {
            _groundChecker.PerformGroundCheck();
            _climbingChecker.PerformWallCheck();
            
            if (!_networkBehaviourAccessor.ParentNetworkBehaviour.GetInput(out PlayerInputData input))
            {
                return;
            }
            
            ProcessRotation(input.LookYawDelta);
            
            if (_groundDetectorDataChanger.IsGrounded)
            {
                _jumpDataChanger.RestartCoyoteTimer(AirborneConfig.CoyoteTimeTicks);
                _jumpDataChanger.ChangeJumpingStatus(false);
            }
                
            if (input.IsJumpPressed)
            {
                _jumpDataChanger.RestartJumpBufferTimer(AirborneConfig.JumpBufferTicks);
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
            
            _jumpDataChanger.ChangeJumpingStatus(true);
            _jumpDataChanger.StopCoyoteTimer();
            _jumpDataChanger.StopJumpBufferTimer();
            
            _mediator.Notify(this, PlayerEventTypes.OnPlayerJump, new JumpPayload());
        }
        
        protected override void BeforePreviousStateExit()
        {
            _mediator.Notify(this, PlayerEventTypes.OnPlayerMovementStateChange,
                new MovementStateChangedPayload() 
                {
                    MovementStates = _movementStateDataChanger.CurrentMovementStates,
                    PreviousMovementStates = _movementStateDataChanger.PreviousMovementStates,
                });
        }
        
        private void ProcessRotation(float yawDelta)
        {
            if (Mathf.Abs(yawDelta) > 0.01f)
            {
                float rotationStep = yawDelta 
                                     * LocomotionConfig.RotationSpeed 
                                     * _networkBehaviourAccessor.ParentNetworkBehaviour.Runner.DeltaTime;
                Quaternion deltaRotation = Quaternion.Euler(0, rotationStep, 0);
                Rigidbody.rotation *= deltaRotation;
            }
        }
    }
}