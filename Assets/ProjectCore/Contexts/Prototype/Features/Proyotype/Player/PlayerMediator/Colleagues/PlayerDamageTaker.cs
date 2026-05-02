using Domain;
using UnityEngine;

namespace Prototype.Prototype
{
    public class PlayerDamageTaker : 
        BaseNetworkEntityComponent, 
        IPlayerColleague
    {
        [SerializeField] private byte _defaultDamage = 10;
        
        private IMediator<IPlayerColleague, PlayerEventTypes> _mediator;
        private INetworkBehaviourAccessor _networkBehaviourAccessor;

        private void Construct(INetworkBehaviourAccessor networkBehaviourAccessor)
        {
            _networkBehaviourAccessor = networkBehaviourAccessor;
        }
        
        public void SetMediator(IMediator<IPlayerColleague, PlayerEventTypes> mediator)
        {
            _mediator = mediator;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_networkBehaviourAccessor.ParentNetworkBehaviour.HasStateAuthority)
            {
                return;
            }
            
            _mediator.Notify(this, PlayerEventTypes.OnPlayerTakeDamage, new DamageTakePayload()
            {
                Amount = _defaultDamage,
                DamageType = "Fire",
            });
        }
    }
}