using Domain;
using UnityEngine;
using Zenject;

namespace Prototype.Prototype
{
    public class PlayerDamageTaker : 
        BaseNetworkEntityComponent, 
        IPlayerColleague
    {
        [SerializeField] private byte _defaultDamage = 10;
        
        private IPlayerMediator _mediator;

        [Inject]
        private void Construct(IPlayerMediator mediator)
        {
            _mediator = mediator;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!ParentNetworkBehaviour.HasStateAuthority)
            {
                return;
            }
            
            _mediator.Notify(new DamageTakePayload()
            {
                Sender = this,
                Amount = _defaultDamage,
                DamageType = "Fire",
            });
        }
    }
}
