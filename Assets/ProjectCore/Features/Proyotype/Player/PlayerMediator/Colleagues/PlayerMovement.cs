using Fusion;
using ProjectCore.Domain.Scripts.Paterns.Mediator;
using ProjectCore.Features.Proyotype.Player.PlayerMediator;
using ProjectCore.Features.Proyotype.Player.PlayerMediator.EventPayloads;
using UnityEngine;

namespace ProjectCore.Features.Proyotype.Player
{
    public class PlayerMovement : NetworkBehaviour, IPlayerColleague
    {
        [SerializeField] private float _speed = 5f;
        
        private IMediator<IPlayerColleague, EPlayerEventType> _mediator;
        private ChangeDetector _changeDetector;
        
        [UnitySerializeField][Networked] private EMovementState MovementState { get; set; } = EMovementState.Idle;
        [UnitySerializeField][Networked] private EMovementState PreviousMovementState { get; set; } = EMovementState.Idle;
        
        public override void Spawned()
        {
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
                Vector3 move = new Vector3(input.Horizontal, 0, input.Vertical);

                if (move.magnitude > 0f)
                {
                    Walk(move);
                }
                else
                {
                    Stop();
                }
            }

            TryToCheckChanges();
        }

        private void Walk(Vector3 direction)
        {
            transform.position += direction * _speed * Runner.DeltaTime;

            TryChangeMovementState(EMovementState.Walk);
        }

        private void Stop()
        {
            TryChangeMovementState(EMovementState.Idle);
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