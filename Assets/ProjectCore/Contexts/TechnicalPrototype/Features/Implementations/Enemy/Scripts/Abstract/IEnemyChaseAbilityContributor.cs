namespace ProjectCore.TechnicalPrototype
{
    public interface IEnemyChaseAbilityContributor : IEnemyAbility
    {
        BaseEnemyChaseAbilityProcessor CreateChaseAbilityProcessor();
    }
}
