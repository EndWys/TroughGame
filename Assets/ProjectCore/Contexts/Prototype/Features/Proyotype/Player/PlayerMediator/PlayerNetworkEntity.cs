using System;
using System.Collections.Generic;
using Domain;
using Fusion;
using UnityEngine;

namespace Prototype.Prototype
{
    public class PlayerNetworkEntity : 
        NetworkBehaviour, INetworkBehaviourAccessor,
        IMediator<IPlayerColleague, PlayerEventTypes>,
        IClimbDetectorDataChanger, IPoseDataChanger, IGroundDetectorDataChanger,
        IHealthDataChanger, IJumpDataChanger, IMovementStateDataChanger<MovementStates>
    {
        [Header("COMPONENTS")]
        [SerializeField] private PlayerCameraTracker _playerCameraTracker;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private PlayerDamageTaker _playerDamageTaker;
        [SerializeField] private PlayerHealth _playerHealth;

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

        private Dictionary<PlayerEventTypes, Action<IPlayerColleague, object>> _handlers;
        public NetworkBehaviour ParentNetworkBehaviour => this;

        public override void Spawned()
        {
            _handlers = new Dictionary<PlayerEventTypes, Action<IPlayerColleague, object>>
            {
                { PlayerEventTypes.OnPlayerMovementStateChange, HandleMovementStateChange },
                { PlayerEventTypes.OnPlayerTakeDamage, HandleDamageTaken },
            };
            
            _playerCameraTracker.SetMediator(this);
            _playerCameraTracker.Init(this);
            
            _playerMovement.SetMediator(this);
            _playerMovement.Init(this);
            
            _playerDamageTaker.Init(this);
            _playerDamageTaker.SetMediator(this);
            
            _playerHealth.SetMediator(this);
            _playerHealth.Init(this);
        }

        public override void FixedUpdateNetwork()
        {
            if (!HasStateAuthority)
            {
                return;
            }
            
            _playerCameraTracker.NetworkTick();
            _playerMovement.NetworkTick();
            _playerDamageTaker.NetworkTick();
            _playerHealth.NetworkTick();
        }

        public override void Render()
        {
            _playerCameraTracker.ClientRender();
            _playerMovement.ClientRender();
            _playerDamageTaker.ClientRender();
            _playerHealth.ClientRender();
        }

        public void Notify(IPlayerColleague sender, PlayerEventTypes eventKey, object args = null)
        {
            if (_handlers.TryGetValue(eventKey, out var handler))
            {
                handler.Invoke(sender, args);
            }
        }

        private void HandleMovementStateChange(IPlayerColleague sender, object args = null)
        {
            if (args is MovementStateChangedPayload payload)
            {
                _playerCameraTracker.ChangeFieldOfView(payload);
            }
        }

        private void HandleDamageTaken(IPlayerColleague sender, object args = null)
        {
            if (args is DamageTakePayload payload)
            {
                _playerHealth.ApplyDamage(payload.Amount);
            }
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