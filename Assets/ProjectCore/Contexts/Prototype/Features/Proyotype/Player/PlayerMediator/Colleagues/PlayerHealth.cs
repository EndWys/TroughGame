using Domain;
using Fusion;

namespace Prototype.Prototype
{
    public class PlayerHealth : NetworkBehaviour, IPlayerColleague
    {
        private IMediator<IPlayerColleague, EPlayerEventType> _mediator;

        [UnitySerializeField][Networked] private int Health { get; set; } = 100;
        
        public void Initialize(IMediator<IPlayerColleague, EPlayerEventType> mediator)
        {
            _mediator = mediator;
        }
        
        public void ApplyDamage(int amount)
        {
            Health -= amount;
        }
    }
}