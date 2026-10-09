using System.Collections.Generic;

namespace ProjectCore.TechnicalPrototype
{
    public interface IEnemyAbilityCollection
    {
        IReadOnlyList<TAbility> GetAbilities<TAbility>() where TAbility : class, IEnemyAbility;
    }
}
