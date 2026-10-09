using System;

namespace ProjectCore.Template
{
    public interface IAppTimeService
    {
        DateTime GetCurrentTime();

        TimeSpan GetTimeDeltaTo(DateTime endTime);

        TimeSpan GetTimeDeltaFrom(DateTime startTime);

        float GetTimeDeltaInSecondsTo(DateTime endTime);

        float GetTimeDeltaInSecondsFrom(DateTime startTime);

        void AddProvider(ITimeProvider provider);

        void RemoveProvider(ITimeProvider provider);
    }
}
