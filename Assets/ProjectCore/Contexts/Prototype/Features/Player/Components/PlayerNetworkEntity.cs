using System.Collections.Generic;
using ProjectCore.GameCore;
using Domain;
using Fusion;
using ProjectCore.GameCore;
using UnityEngine;
using UnityEngine.Serialization;

namespace ProjectCore.Prototype
{
    public class PlayerNetworkEntity : 
        BaseNetworkEntityRoot,
        IPlayerDataHolder
    {
        [Header("COMPONENTS")]
        [FormerlySerializedAs("_groundChecker")]
        [SerializeField] private GroundDetectorComponent _groundDetectorComponent;
        [FormerlySerializedAs("_climbingChecker")]
        [SerializeField] private ClimbDetectorComponent _climbDetectorComponent;
        [FormerlySerializedAs("_poseController")]
        [SerializeField] private PlayerPoseComponent _playerPoseComponent;
        [SerializeField] private PlayerMediator _playerMediator;

        [Header("NETWORKED DATA")]
        
        [UnitySerializeField, Networked] public int Health { get; private set; }
        
        [UnitySerializeField, Networked] public bool IsGrounded { get; private set; }
        [UnitySerializeField, Networked] public Vector3 GroundNormal { get; private set; }

        [UnitySerializeField, Networked] public bool IsNearValidWall { get; private set; }
        [UnitySerializeField, Networked] public Vector3 CurrentWallNormal { get; private set; }
        
        [UnitySerializeField, Networked] public PoseTypes CurrentPose { get; private set; }

        [UnitySerializeField, Networked] public bool IsJumping { get; private set; }
        [UnitySerializeField, Networked] public TickTimer CoyoteTimer { get; private set; } = TickTimer.None;
        [UnitySerializeField, Networked] public TickTimer JumpBufferTimer { get; private set; } = TickTimer.None;
        
        [UnitySerializeField, Networked] public MovementStates CurrentMovementStates { get; private set; }
        [UnitySerializeField, Networked] public MovementStates PreviousMovementStates { get; private set; }

        public MovementStates CurrentState => CurrentMovementStates;
        public MovementStates PreviousState => PreviousMovementStates;

        protected override void BeforeComponentsInitialized()
        {
            if (HasStateAuthority || HasInputAuthority)
            {
                Runner.SetIsSimulated(Object, true);
            }
        }

        protected override IEnumerable<INetworkEntityComponent> CreateComponents()
        {
            return new INetworkEntityComponent[]
            {
                _groundDetectorComponent,
                _climbDetectorComponent,
                _playerPoseComponent,
                _playerMediator,
            };
        }
        
        public void ChangePose(PoseTypes newPose)
        {
            CurrentPose = newPose;
        }
        
        public void ChangeMovementState(MovementStates newStates)
        {
            PreviousMovementStates = CurrentMovementStates;
            CurrentMovementStates = newStates;
        }

        public void ChangeState(MovementStates newState)
        {
            ChangeMovementState(newState);
        }

        public void ChangeWallAvailability(bool hasAvailableWall)
        {
            IsNearValidWall = hasAvailableWall;
        }

        public void ChangeCurrentWallNormal(Vector3 newWallNormal)
        {
            CurrentWallNormal = newWallNormal;
        }

        public void ChangeGroundedStatus(bool isGrounded)
        {
            IsGrounded = isGrounded;
        }

        public void ChangeGroundNormal(Vector3 groundNormal)
        {
            GroundNormal = groundNormal;
        }

        public void ReduceHealth(byte amount)
        {
            Health -= amount;
        }

        public void RestoreHealth(byte amount)
        {
            Health += amount;
        }

        public void ChangeJumpingStatus(bool isJumping)
        {
            IsJumping = isJumping;
        }

        public void RestartCoyoteTimer(int ticks)
        {
            CoyoteTimer = TickTimer.CreateFromTicks(Runner, ticks);
        }

        public void RestartJumpBufferTimer(int ticks)
        {
            JumpBufferTimer = TickTimer.CreateFromTicks(Runner, ticks);
        }

        public void StopCoyoteTimer()
        {
            CoyoteTimer = TickTimer.None;
        }

        public void StopJumpBufferTimer()
        {
            JumpBufferTimer = TickTimer.None;
        }
    }
}
