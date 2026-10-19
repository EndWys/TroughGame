using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public interface IPlayerInputDirectionMutator : IPlayerInputDirectionAccessor
    {
        void SetLatestInputDirection(Vector2 direction);
    }
}
