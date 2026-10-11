using ProjectCore.GameCore;

namespace ProjectCore.TechnicalPrototype
{
    public interface IEnemyTargetAccessor
    {
        NetworkEntityIdData TargetEntityId { get; }
    }
}
