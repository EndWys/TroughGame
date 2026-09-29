using System;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public static class EnemyAbilityValidation
    {
        public static void ValidateAbilityComponent(MonoBehaviour abilityComponent)
        {
            if (abilityComponent == null)
            {
                throw new InvalidOperationException(
                    "Enemy ability collection must not contain null references.");
            }

            if (abilityComponent is IEnemyAbility)
            {
                return;
            }

            throw new InvalidOperationException(
                string.Concat(
                    "Enemy ability component '",
                    abilityComponent.GetType().Name,
                    "' must implement ",
                    nameof(IEnemyAbility),
                    "."));
        }
    }
}
