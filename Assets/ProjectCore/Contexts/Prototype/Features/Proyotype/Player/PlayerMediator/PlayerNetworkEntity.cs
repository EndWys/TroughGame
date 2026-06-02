using System;
using System.Collections.Generic;
using Domain;
using Fusion;
using UnityEngine;

namespace Prototype.Prototype
{
    public class PlayerNetworkEntity : 
        NetworkBehaviour, INetworkBehaviourAccessor,
        IPlayerMediator,
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

        private List<Action<HandlerPayload>> _handlers;
        private INetworkEntityComponent[] _components;
        public NetworkBehaviour ParentNetworkBehaviour => this;

        public override void Spawned()
        {
            if (HasStateAuthority || HasInputAuthority)
            {
                Runner.SetIsSimulated(Object, true);
            }
            
            _handlers = new List<Action<HandlerPayload>>
            {
                HandleMovementStateChange,
                HandleDamageTaken,
            };

            _components = new INetworkEntityComponent[]
            {
                _playerCameraTracker,
                _playerMovement,
                _playerDamageTaker,
                _playerHealth,
            };

            foreach (INetworkEntityComponent component in _components)
            {
                component.Init();
            }
        }

        public override void FixedUpdateNetwork()
        {
            foreach (INetworkEntityComponent component in _components)
            {
                component.NetworkTick();
            }
        }

        public override void Render()
        {
            foreach (INetworkEntityComponent component in _components)
            {
                component.ClientRender();
            }
        }

        public void Notify(HandlerPayload payload)
        {
            foreach (Action<HandlerPayload> handler in _handlers)
            {
                handler.Invoke(payload);
            }
        }

        private void HandleMovementStateChange(HandlerPayload payload)
        {
            if (payload is MovementStateChangedPayload movementStateChangedPayload)
            {
                _playerCameraTracker.ChangeFieldOfView(movementStateChangedPayload);
            }
        }

        private void HandleDamageTaken(HandlerPayload payload)
        {
            if (payload is DamageTakePayload damageTakePayload)
            {
                _playerHealth.ApplyDamage(damageTakePayload.Amount);
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
