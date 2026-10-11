using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using Shared;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace ProjectCore.Template
{
    public sealed class SceneFlowServiceTests
    {
        private TestSceneDefinition _definition;
        private SceneCatalogConfig _catalog;
        private SceneFlowService _service;
        private TestDebugLogger _logger;

        [SetUp]
        public void SetUp()
        {
            _definition = ScriptableObject.CreateInstance<TestSceneDefinition>();
            var serializedDefinition = new SerializedObject(_definition);
            serializedDefinition.FindProperty("_buildIndex").intValue = 0;
            serializedDefinition.ApplyModifiedPropertiesWithoutUndo();

            _catalog = ScriptableObject.CreateInstance<SceneCatalogConfig>();
            var serializedCatalog = new SerializedObject(_catalog);
            SerializedProperty definitions = serializedCatalog.FindProperty("_definitions");
            definitions.arraySize = 1;
            definitions.GetArrayElementAtIndex(0).objectReferenceValue = _definition;
            serializedCatalog.ApplyModifiedPropertiesWithoutUndo();

            _logger = new TestDebugLogger();
        }

        [TearDown]
        public void TearDown()
        {
            _service?.Dispose();
            UnityEngine.Object.DestroyImmediate(_catalog);
            UnityEngine.Object.DestroyImmediate(_definition);
        }

        [Test]
        public void TransitionStaysLockedUntilPresenterHideCompletes()
        {
            var showException = new InvalidOperationException("Show failed.");
            var hideCompletion = new UniTaskCompletionSource();
            CreateService(new TestTransitionPresenter(
                _ => throw showException,
                _ => hideCompletion.Task));

            UniTask<Result> operation = Load(CancellationToken.None);

            try
            {
                Assert.That(operation.Status, Is.EqualTo(UniTaskStatus.Pending));
                Assert.That(_service.IsTransitioning, Is.True);

                Result secondResult = Load(CancellationToken.None).GetAwaiter().GetResult();

                Assert.That(secondResult.IsFailure, Is.True);
                Assert.That(secondResult.FirstError.Code,
                    Is.EqualTo("SceneFlow.TransitionInProgress"));
                Assert.That(_service.IsTransitioning, Is.True);
            }
            finally
            {
                hideCompletion.TrySetResult();
            }

            Result result = operation.GetAwaiter().GetResult();

            Assert.That(result.IsFailure, Is.True);
            StringAssert.Contains(showException.Message, result.FirstError.ExceptionDetails);
            Assert.That(_service.IsTransitioning, Is.False);
        }

        [Test]
        public void HideFailuresAreLoggedWithoutReplacingTransitionFailureOrStoppingCleanup()
        {
            var showException = new InvalidOperationException("Transition failed.");
            var lastHideException = new InvalidOperationException("Last hide failed.");
            var middleHideException = new InvalidOperationException("Middle hide failed.");
            var hiddenPresenters = new List<string>();
            CreateService(
                new TestTransitionPresenter(
                    _ => UniTask.CompletedTask,
                    _ =>
                    {
                        hiddenPresenters.Add("First");
                        return UniTask.CompletedTask;
                    }),
                new TestTransitionPresenter(
                    _ => UniTask.CompletedTask,
                    _ =>
                    {
                        hiddenPresenters.Add("Middle");
                        throw middleHideException;
                    }),
                new TestTransitionPresenter(
                    _ => throw showException,
                    _ =>
                    {
                        hiddenPresenters.Add("Last");
                        throw lastHideException;
                    }));

            Result result = Load(CancellationToken.None).GetAwaiter().GetResult();

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.FirstError.Code, Is.EqualTo("SceneFlow.TransitionFailed"));
            StringAssert.Contains(showException.Message, result.FirstError.ExceptionDetails);
            CollectionAssert.AreEqual(new[] { "Last", "Middle", "First" }, hiddenPresenters);
            CollectionAssert.AreEqual(
                new[] { lastHideException, middleHideException }, _logger.Exceptions);
            Assert.That(_service.IsTransitioning, Is.False);
        }

        [Test]
        public void HideFailurePreservesCancellationAndUsesUncancelledCleanupToken()
        {
            using var cancellation = new CancellationTokenSource();
            var hideException = new InvalidOperationException("Hide failed after cancellation.");
            CancellationToken hideToken = default;
            CreateService(new TestTransitionPresenter(
                token =>
                {
                    cancellation.Cancel();
                    throw new OperationCanceledException(token);
                },
                token =>
                {
                    hideToken = token;
                    throw hideException;
                }));

            UniTask<Result> operation = Load(cancellation.Token);
            OperationCanceledException exception = Assert.Throws<OperationCanceledException>(
                () => operation.GetAwaiter().GetResult());

            Assert.That(exception.CancellationToken, Is.EqualTo(cancellation.Token));
            Assert.That(hideToken.CanBeCanceled, Is.False);
            CollectionAssert.AreEqual(new[] { hideException }, _logger.Exceptions);
            Assert.That(_service.IsTransitioning, Is.False);
        }

        private void CreateService(params ISceneTransitionPresenter[] presenters)
        {
            // These tests fail or cancel during Show, before the Unity loader is used.
            _service = new SceneFlowService(
                new ZenjectSceneLoader(null, null),
                new List<ISceneTransitionPresenter>(presenters),
                _logger);
            _service.Initialize(_catalog);
        }

        private UniTask<Result> Load(CancellationToken cancellationToken)
        {
            return _service.LoadAsync(_definition, new TestSceneSettings(), cancellationToken);
        }

        private sealed class TestSceneDefinition : BaseSceneDefinition
        {
            public override Type SceneType => typeof(TestScene);
            public override Type SettingsType => typeof(TestSceneSettings);
        }

        private sealed class TestScene : IScene<TestSceneSettings>
        {
        }

        private sealed class TestSceneSettings : ISceneSettings
        {
        }

        private sealed class TestTransitionPresenter : ISceneTransitionPresenter
        {
            private readonly Func<CancellationToken, UniTask> _show;
            private readonly Func<CancellationToken, UniTask> _hide;

            public TestTransitionPresenter(
                Func<CancellationToken, UniTask> show,
                Func<CancellationToken, UniTask> hide)
            {
                _show = show;
                _hide = hide;
            }

            public UniTask ShowAsync(CancellationToken cancellationToken)
            {
                return _show(cancellationToken);
            }

            public UniTask HideAsync(CancellationToken cancellationToken)
            {
                return _hide(cancellationToken);
            }
        }

        private sealed class TestDebugLogger : IDebugLogger<SceneFlowFeature>
        {
            public List<Exception> Exceptions { get; } = new List<Exception>();

            public void LogMessage(string message)
            {
            }

            public void LogWarning(string message)
            {
            }

            public void LogError(string message)
            {
            }

            public void LogException(Exception exception, string message = null)
            {
                Exceptions.Add(exception);
            }
        }
    }
}
