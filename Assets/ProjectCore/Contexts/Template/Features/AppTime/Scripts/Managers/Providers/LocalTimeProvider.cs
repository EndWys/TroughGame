using System;

namespace ProjectCore.Template
{
    public sealed class LocalTimeProvider : ITimeProvider
    {
        public DateTime GetCurrentTime()
        {
            return DateTime.UtcNow;
        }
    }
}
