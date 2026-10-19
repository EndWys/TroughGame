namespace ProjectCore.GameCore
{
    public interface ICombatActionMutator : ICombatActionAccessor
    {
        void SetActionState(in CombatActionStateModel state);
    }
}
