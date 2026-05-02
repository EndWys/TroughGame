namespace Prototype.Prototype
{
    public enum PlayerEventTypes : byte
    {
        //Functional Events - Changing Data.
        OnPlayerMovementStateChange,
        OnPlayerJump,
        OnPlayerGroundingStateChange,
        OnPlayerGroundNormalChange,
        OnPlayerTakeDamage,
        OnPlayerHealthChange,
        OnPlayerDeath,
        
        //Render Events - Casting VFX, Sounds, Visualization.
        OnRenderPlayerTakeDamage
    }
}