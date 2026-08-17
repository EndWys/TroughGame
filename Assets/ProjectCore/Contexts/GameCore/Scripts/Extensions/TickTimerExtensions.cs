using Fusion;

namespace ProjectCore.GameCore
{
    public static class TickTimerExtensions
    {
        public static bool IsEnabled(this TickTimer tickTimer, NetworkRunner runner)
        {
            return tickTimer.ExpiredOrNotRunning(runner);
        }
        
        public static bool IsDisabled(this TickTimer tickTimer, NetworkRunner runner)
        {
            return !IsEnabled(tickTimer, runner);
        }
    }
}
