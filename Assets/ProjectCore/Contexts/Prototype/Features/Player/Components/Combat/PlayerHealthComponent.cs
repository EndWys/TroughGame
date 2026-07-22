using Domain;
using GameCore.Combat;
using Zenject;

namespace Prototype.Prototype
{
    public class PlayerHealthComponent : 
        BaseDamageableComponent,
        IPlayerColleague
    {
        private IHealthDataChanger _healthDataChanger;

        [Inject]
        private void Construct(IHealthDataChanger healthDataChanger)
        {
            _healthDataChanger = healthDataChanger;
        }

        public override int Durability => _healthDataChanger.Health;

        public override bool IsDestroyed => Durability <= 0;
        
        public override void ApplyDamage(byte amount)
        {
            _healthDataChanger.ReduceHealth(amount);
        }
    }
}
