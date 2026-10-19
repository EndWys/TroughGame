using Shared;
using System.Collections.Generic;
using ProjectCore.GameCore;

namespace ProjectCore.TechnicalPrototype
{
    public interface IEnemyEntityStateContributor : IEnemyAbility
    {
        void AddEntityStates(
            IDictionary<EnemyEntityState, BaseEntityState<EnemyEntityState, EnemyEntityPayload>>
                entityStates);

        void AddTransitionProcessors(
            EnemyEntityState sourceState,
            IList<IStateProcessor<EnemyEntityState, EnemyEntityPayload>> processors);
    }
}
