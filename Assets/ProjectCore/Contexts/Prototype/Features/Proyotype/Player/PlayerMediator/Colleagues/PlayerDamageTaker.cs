using Domain;
using UnityEngine;

namespace Prototype.Prototype
{
    public class PlayerDamageTaker : 
        BaseNetworkEntityComponent, 
        IPlayerColleague
    {
        [SerializeField] private byte _defaultDamage = 10;
        
        private IMediator _mediator;
        private INetworkBehaviourAccessor _networkBehaviourAccessor;

        private void Construct(INetworkBehaviourAccessor networkBehaviourAccessor)
        {
            _networkBehaviourAccessor = networkBehaviourAccessor;
        }
        
        public void SetMediator(IMediator mediator)
        {
            _mediator = mediator;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_networkBehaviourAccessor.ParentNetworkBehaviour.HasStateAuthority)
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
