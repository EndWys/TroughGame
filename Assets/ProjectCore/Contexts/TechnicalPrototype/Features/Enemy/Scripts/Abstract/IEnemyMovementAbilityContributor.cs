using System.Collections.Generic;
using ProjectCore.GameCore;

namespace ProjectCore.TechnicalPrototype
{
    public interface IEnemyMovementAbilityContributor : IEnemyAbility
    {
        void AddMovementStates(
            IDictionary<EnemyMovementState, BaseMovementState<EnemyMovementState, EnemyMovementPayload>>
                movementStates);

        void AddTransitionProcessors(
            EnemyMovementState sourceMovementState,
            IList<IMovementStateProcessor<EnemyMovementState, EnemyMovementPayload>> processors);
    }
}
