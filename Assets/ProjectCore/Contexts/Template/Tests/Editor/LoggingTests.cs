using System;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Zenject;

namespace ProjectCore.Template
{
    public sealed class LoggingTests
    {
        [Test]
        public void FeatureLoggerPrefixesMessagesWithFeatureName()
        {
            var targetLogger = new TestDebugLogger();
            IDebugLogger<TestFeature> logger = new FeatureDebugLoggerService<TestFeature>(targetLogger);

            logger.LogWarning("NTP unavailable.");

            Assert.AreEqual("[Test] NTP unavailable.", targetLogger.Warning);
        }

        [Test]
        public void FeatureLoggerIsSharedWithinContextAndSeparatedBetweenContexts()
        {
            var firstContext = CreateContext();
            var secondContext = CreateContext();

            IDebugLogger<TestFeature> first = firstContext.Resolve<IDebugLogger<TestFeature>>();
            IDebugLogger<TestFeature> repeated = firstContext.Resolve<IDebugLogger<TestFeature>>();
            IDebugLogger<TestFeature> anotherContext = secondContext.Resolve<IDebugLogger<TestFeature>>();

            Assert.AreSame(first, repeated);
            Assert.AreNotSame(first, anotherContext);
        }

        [Test]
        public void LogServiceSubscribesOnlyOnceAndUnsubscribesOnDispose()
        {
            const string capturedMessage = "LoggingTests.Captured";
            const string ignoredMessage = "LoggingTests.Ignored";
            var handler = new TestLogHandler(capturedMessage);
            var service = new LogService();
            service.AddLogHandler(handler);
            service.Initialize();
            service.Initialize();

            LogAssert.Expect(LogType.Log, capturedMessage);
            Debug.Log(capturedMessage);
            Assert.AreEqual(1, handler.HandledCount);

            service.Dispose();
            LogAssert.Expect(LogType.Log, ignoredMessage);
            Debug.Log(ignoredMessage);
            Assert.AreEqual(1, handler.HandledCount);
        }

        private static DiContainer CreateContext()
        {
            var container = new DiContainer();
            container.Bind<IDebugLogger>().To<TestDebugLogger>().AsSingle();
            container.Bind(typeof(IDebugLogger<>))
                .To(typeof(FeatureDebugLoggerService<>))
                .AsCached();
            return container;
        }

        private sealed class TestDebugLogger : IDebugLogger
        {
            public string Warning { get; private set; }

            public void LogMessage(string message)
            {
            }

            public void LogWarning(string message)
            {
                Warning = message;
            }

            public void LogError(string message)
            {
            }

            public void LogException(Exception exception, string message = null)
            {
            }
        }

        private sealed class TestLogHandler : ILogHandler
        {
            private readonly string _expectedMessage;

            public TestLogHandler(string expectedMessage)
            {
                _expectedMessage = expectedMessage;
            }

            public int HandledCount { get; private set; }

            public void HandleLog(string condition, string stackTrace, LogType type)
            {
                if (condition == _expectedMessage)
                {
                    HandledCount++;
                }
            }
        }

        private sealed class TestFeature
        {
        }
    }
}
