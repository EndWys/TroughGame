using Domain;

namespace ProjectCore.TechnicalPrototype
{
    public interface IEnemyBehaviourStateMutator :
        IEnemyBehaviourStateAccessor,
        IStateDataMutator<EnemyBehaviourStateType>
    {
    }
}
