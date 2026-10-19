using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public interface IPlayerInputDirectionMutator
    {
        Vector2 LatestInputDirection { get; }
        void SetLatestInputDirection(Vector2 direction);
    }
}
