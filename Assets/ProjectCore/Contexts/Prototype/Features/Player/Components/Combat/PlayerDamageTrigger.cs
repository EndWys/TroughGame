using Domain;
using ProjectCore.GameCore;
using UnityEngine;
using Zenject;

namespace ProjectCore.Prototype
{
    public class PlayerDamageTrigger : 
        BaseDamageSourceComponent, 
        IPlayerColleague
    {
        [SerializeField] private byte _defaultDamage = 10;
        
        private IPlayerMediator _mediator;

        public override byte DamageAmount => _defaultDamage;

        public override string DamageType => "Fire";

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
                Amount = DamageAmount,
                DamageType = DamageType,
            });
        }
    }
}
