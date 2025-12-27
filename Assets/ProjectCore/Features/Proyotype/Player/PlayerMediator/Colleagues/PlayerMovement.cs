using Fusion;
using ProjectCore.Domain.Scripts.Paterns.Mediator;
using ProjectCore.Features.Proyotype.Player.PlayerMediator;
using UnityEngine;

namespace ProjectCore.Features.Proyotype.Player
{
    public class PlayerMovement : NetworkBehaviour, IPlayerColleague
    {
        [SerializeField] private float _speed = 5f;
        
        private IMediator<IPlayerColleague, EPlayerEventType> _mediator;
        
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
            }
        }

        private void Walk(Vector3 direction)
        {
            transform.position += direction * _speed * Runner.DeltaTime;
                    
            _mediator.Notify(this, EPlayerEventType.OnPlayerWalk);
        }
    }
}