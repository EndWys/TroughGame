using Domain;
using UnityEngine;
using Zenject;

namespace Prototype.Prototype
{
    public class PlayerDamageTrigger : 
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
            
            _mediator.Notify(new PlayerDamageTakenPayload()
            {
                Sender = this,
                Amount = _defaultDamage,
                DamageType = "Fire",
            });
        }
    }
}
