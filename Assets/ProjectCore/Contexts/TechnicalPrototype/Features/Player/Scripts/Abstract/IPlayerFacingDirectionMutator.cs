namespace ProjectCore.TechnicalPrototype
{
    public interface IPlayerFacingDirectionMutator : IPlayerFacingDirectionAccessor
    {
        void SetFacingDirection(PlayerFacingDirection facingDirection);
    }
}
