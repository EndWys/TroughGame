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
        private INetworkBehaviourAccessor _networkBehaviourAccessor;

        [Inject]
        private void Construct(INetworkBehaviourAccessor networkBehaviourAccessor, IPlayerMediator mediator)
        {
            _networkBehaviourAccessor = networkBehaviourAccessor;
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
