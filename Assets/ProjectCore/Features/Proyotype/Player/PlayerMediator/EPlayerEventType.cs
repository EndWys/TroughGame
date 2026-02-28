namespace ProjectCore.Features.Prototype.Player.PlayerMediator
{
    public enum EPlayerEventType : byte
    {
        //Functional Events - Changing Data.
        OnPlayerMovementStateChange,
        OnPlayerGroundingStateChange,
        OnPlayerGroundNormalChange,
        OnPlayerTakeDamage,
        OnPlayerHealthChange,
        OnPlayerDeath,
        
        //Render Events - Casting VFX, Sounds, Visualization.
        OnRenderPlayerTakeDamage
    }
}