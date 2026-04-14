using Domain;
using Fusion;
using UnityEngine;

namespace Prototype.Prototype
{
    public class PlayerDamageTaker : NetworkBehaviour, IPlayerColleague
    {
        [SerializeField] private int _defaultDamage = 10;
        
        private IMediator<IPlayerColleague, EPlayerEventType> _mediator;
        
        public void Init(IMediator<IPlayerColleague, EPlayerEventType> mediator)
        {
            _mediator = mediator;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!HasStateAuthority) return;
            
            _mediator.Notify(this, EPlayerEventType.OnPlayerTakeDamage, new DamageTakePayload()
            {
                Amount = _defaultDamage,
                DamageType = "Fire",
            });
        }
    }
}