using Domain;
using Zenject;

namespace Prototype.Prototype
{
    public class PlayerHealth : 
        BaseNetworkEntityComponent,
        IPlayerColleague
    {
        private IMediator _mediator;
        private IHealthDataChanger _healthDataChanger;

        [Inject]
        private void Construct(IHealthDataChanger healthDataChanger)
        {
            _healthDataChanger = healthDataChanger;
        }
        
        public void SetMediator(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        public void ApplyDamage(byte amount)
        {
            _healthDataChanger.ReduceHealth(amount);
        }
    }
}
