using Domain;
using Zenject;

namespace Prototype.Prototype
{
    public class PlayerHealthComponent : 
        BaseNetworkEntityComponent,
        IPlayerColleague
    {
        private IHealthDataChanger _healthDataChanger;

        [Inject]
        private void Construct(IHealthDataChanger healthDataChanger)
        {
            _healthDataChanger = healthDataChanger;
        }
        
        public void ApplyDamage(byte amount)
        {
            _healthDataChanger.ReduceHealth(amount);
        }
    }
}
