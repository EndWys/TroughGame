using ProjectCore.GameCore;

namespace ProjectCore.TechnicalPrototype
{
    public interface IEnemyTargetMutator : IEnemyTargetAccessor
    {
        void SetTargetEntityId(NetworkEntityIdData targetEntityId);

        void ClearTargetEntityId();
    }
}
