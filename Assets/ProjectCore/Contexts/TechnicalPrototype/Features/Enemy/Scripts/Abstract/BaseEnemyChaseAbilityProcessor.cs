using ProjectCore.GameCore;
using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public abstract class BaseEnemyChaseAbilityProcessor
    {
        public abstract bool TryCreateInput(
            NetworkEntityIdData targetEntityId,
            Vector2 targetPosition,
            Vector2 approachPosition,
            out EnemyInputData input);
    }
}
