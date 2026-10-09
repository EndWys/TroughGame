namespace ProjectCore.TechnicalPrototype
{
    public interface IEnemyBehaviourStateAccessor
    {
        EnemyBehaviourStateType CurrentBehaviourState { get; }

        EnemyBehaviourStateType PreviousBehaviourState { get; }
    }
}
