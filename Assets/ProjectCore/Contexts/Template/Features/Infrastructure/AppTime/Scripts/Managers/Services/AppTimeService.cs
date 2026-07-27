using System;
using System.Collections.Generic;

namespace ProjectCore.Template
{
    public sealed class AppTimeService : IAppTimeService
    {
        private readonly List<ITimeProvider> _providers = new List<ITimeProvider>();

        public DateTime GetCurrentTime()
        {
            if (_providers.Count == 0)
            {
                throw new InvalidOperationException(
                    "AppTimeService requires at least one time provider.");
            }

            return _providers[_providers.Count - 1].GetCurrentTime();
        }

        public TimeSpan GetTimeDeltaTo(DateTime endTime)
        {
            return endTime - GetCurrentTime();
        }

        public TimeSpan GetTimeDeltaFrom(DateTime startTime)
        {
            return GetCurrentTime() - startTime;
        }

        public float GetTimeDeltaInSecondsTo(DateTime endTime)
        {
            return (float)GetTimeDeltaTo(endTime).TotalSeconds;
        }

        public float GetTimeDeltaInSecondsFrom(DateTime startTime)
        {
            return (float)GetTimeDeltaFrom(startTime).TotalSeconds;
        }

        public void AddProvider(ITimeProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            if (!_providers.Contains(provider))
            {
                _providers.Add(provider);
            }
        }

        public void RemoveProvider(ITimeProvider provider)
        {
            if (provider == null)
            {
                throw new ArgumentNullException(nameof(provider));
            }

            _providers.Remove(provider);
        }
    }
}
