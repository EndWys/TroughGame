using Fusion;
using ProjectCore.Domain.Scripts.Paterns.Mediator;
using ProjectCore.Features.Proyotype.Player.PlayerMediator;
using ProjectCore.Features.Proyotype.Player.PlayerMediator.EventPayloads;
using UnityEngine;

namespace ProjectCore.Features.Proyotype.Player
{
    public class PlayerDamageTaker : NetworkBehaviour, IPlayerColleague
    {
        [SerializeField] private int _defaultDamage = 10;
        
        private IMediator<IPlayerColleague, EPlayerEventType> _mediator;
        
        public void Initialize(IMediator<IPlayerColleague, EPlayerEventType> mediator)
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