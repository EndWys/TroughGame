using Domain;
using ProjectCore.GameCore;
using Zenject;

namespace ProjectCore.Prototype
{
    public class PlayerHealthComponent : 
        BaseDamageableComponent,
        IPlayerColleague
    {
        private IHealthDataMutator _healthDataMutator;

        [Inject]
        private void Construct(IHealthDataMutator healthDataMutator)
        {
            _healthDataMutator = healthDataMutator;
        }

        public override int Durability => _healthDataMutator.Health;

        public override bool IsDestroyed => Durability <= 0;
        
        public override void ApplyDamage(byte amount)
        {
            _healthDataMutator.ReduceHealth(amount);
        }
    }
}
