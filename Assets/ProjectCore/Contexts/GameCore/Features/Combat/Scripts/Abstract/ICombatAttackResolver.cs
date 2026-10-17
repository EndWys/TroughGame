namespace ProjectCore.GameCore
{
    public interface ICombatAttackResolver
    {
        bool TryGetAttack(ushort actionId, out CombatAttackConfig attack);
    }
}
