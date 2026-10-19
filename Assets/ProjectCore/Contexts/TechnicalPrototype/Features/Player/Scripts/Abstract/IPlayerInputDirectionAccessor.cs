using UnityEngine;

namespace ProjectCore.TechnicalPrototype
{
    public interface IPlayerInputDirectionAccessor
    {
        Vector2 LatestInputDirection { get; }
    }
}
