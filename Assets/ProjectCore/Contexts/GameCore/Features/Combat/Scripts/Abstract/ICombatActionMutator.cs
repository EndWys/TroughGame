using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface ICombatActionMutator : ICombatActionAccessor
    {
        void SetActionState(in CombatActionStateModel state);
        void SetLatestInputDirection(Vector2 direction);
    }
}
