namespace ProjectCore.GameCore
{
    public interface ICombatHurtbox
    {
        bool IsAvailable { get; }
        CombatShapeData WorldShape { get; }
    }
}
