using System;
using NUnit.Framework;

namespace ProjectCore.Template
{
    public sealed class AppTimeServiceTests
    {
        [Test]
        public void CurrentTimeUsesTheLastRegisteredProvider()
        {
            var service = new AppTimeService();
            var firstProvider = new FixedTimeProvider(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));
            var secondProvider = new FixedTimeProvider(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));

            service.AddProvider(firstProvider);
            service.AddProvider(secondProvider);

            Assert.AreEqual(secondProvider.GetCurrentTime(), service.GetCurrentTime());
        }

        [Test]
        public void TimeDeltaUsesUtcProviderTime()
        {
            var currentTime = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
            var service = new AppTimeService();
            service.AddProvider(new FixedTimeProvider(currentTime));

            Assert.AreEqual(
                TimeSpan.FromHours(2),
                service.GetTimeDeltaFrom(currentTime - TimeSpan.FromHours(2)));
            Assert.AreEqual(
                TimeSpan.FromHours(2),
                service.GetTimeDeltaTo(currentTime + TimeSpan.FromHours(2)));
        }

        [Test]
        public void RemovingTheOnlyProviderMakesServiceFailClearly()
        {
            var service = new AppTimeService();
            var provider = new FixedTimeProvider(DateTime.UtcNow);
            service.AddProvider(provider);
            service.RemoveProvider(provider);

            Assert.Throws<InvalidOperationException>(() => service.GetCurrentTime());
        }

        private sealed class FixedTimeProvider : ITimeProvider
        {
            private readonly DateTime _time;

            public FixedTimeProvider(DateTime time)
            {
                _time = time;
            }

            public DateTime GetCurrentTime()
            {
                return _time;
            }
        }
    }
}
