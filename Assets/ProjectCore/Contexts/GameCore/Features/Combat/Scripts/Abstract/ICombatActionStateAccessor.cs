using UnityEngine;

namespace ProjectCore.GameCore
{
    public interface ICombatActionStateAccessor
    {
        CombatActionStateModel ActionState { get; }
        Vector2 LatestInputDirection { get; }
    }
}
