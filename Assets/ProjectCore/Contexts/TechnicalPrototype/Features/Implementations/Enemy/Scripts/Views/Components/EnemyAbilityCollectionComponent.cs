using System;
using System.Collections.Generic;
using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public sealed class EnemyAbilityCollectionComponent :
        BaseNetworkEntityComponent,
        IEnemyAbilityCollection
    {
        [SerializeField] private MonoBehaviour[] _abilityComponents = Array.Empty<MonoBehaviour>();

        private IReadOnlyList<IEnemyAbility> _abilities;

        public IReadOnlyList<MonoBehaviour> AbilityComponents =>
            _abilityComponents ?? Array.Empty<MonoBehaviour>();

        public override void Init()
        {
            MonoBehaviour[] abilityComponents = _abilityComponents ?? Array.Empty<MonoBehaviour>();
            var abilities = new List<IEnemyAbility>(abilityComponents.Length);

            foreach (MonoBehaviour abilityComponent in abilityComponents)
            {
                EnemyAbilityValidation.ValidateAbilityComponent(abilityComponent);
                abilities.Add((IEnemyAbility)abilityComponent);
            }

            _abilities = abilities;
        }

        public IReadOnlyList<TAbility> GetAbilities<TAbility>() where TAbility : class, IEnemyAbility
        {
            var abilities = new List<TAbility>();

            foreach (IEnemyAbility ability in GetAbilities())
            {
                if (ability is TAbility typedAbility)
                {
                    abilities.Add(typedAbility);
                }
            }

            return abilities;
        }

        private IReadOnlyList<IEnemyAbility> GetAbilities()
        {
            return _abilities ?? throw new InvalidOperationException(
                "Enemy ability collection is not initialized.");
        }
    }
}
