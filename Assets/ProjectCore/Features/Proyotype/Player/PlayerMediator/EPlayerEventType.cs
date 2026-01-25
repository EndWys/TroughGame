namespace ProjectCore.Features.Prototype.Player.PlayerMediator
{
    public enum EPlayerEventType
    {
        //Functional Events - Changing Data.
        OnPlayerMovementStateChange,
        OnPlayerTakeDamage,
        OnPlayerHealthChange,
        OnPlayerDeath,
        
        //Render Events - Casting VFX, Sounds, Visualization.
        OnRenderPlayerTakeDamage
    }
}