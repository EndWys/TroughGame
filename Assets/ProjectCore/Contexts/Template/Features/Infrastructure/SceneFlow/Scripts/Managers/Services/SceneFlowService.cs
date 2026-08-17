using Cysharp.Threading.Tasks;
using Domain;
using System;
using System.Collections.Generic;
using System.Threading;
using Zenject;

namespace ProjectCore.Template
{
    public sealed class SceneFlowService : ISceneFlowService, IDisposable
    {
        private readonly ZenjectSceneLoader _sceneLoader;
        private readonly List<ISceneTransitionPresenter> _transitionPresenters;
        private readonly Dictionary<Type, BaseSceneDefinition> _definitions =
            new Dictionary<Type, BaseSceneDefinition>();

        private CancellationTokenSource _currentSceneCancellation;
        private IGameSceneLifecycle _currentSceneLifecycle;
        private bool _isInitialized;

        public SceneFlowService(
            ZenjectSceneLoader sceneLoader,
            List<ISceneTransitionPresenter> transitionPresenters)
        {
            _sceneLoader = sceneLoader;
            _transitionPresenters = transitionPresenters;
        }

        public bool IsTransitioning { get; private set; }
        public BaseSceneDefinition CurrentSceneDefinition { get; private set; }

        public void Initialize(SceneCatalogConfig sceneCatalog)
        {
            if (_isInitialized)
            {
                throw new InvalidOperationException("SceneFlowService is already initialized.");
            }

            if (sceneCatalog == null)
            {
                throw new ArgumentNullException(nameof(sceneCatalog));
            }

            foreach (BaseSceneDefinition definition in sceneCatalog.Definitions)
            {
                if (definition == null)
                {
                    throw new InvalidOperationException("Scene catalog contains an empty definition.");
                }

                if (definition.BuildIndex < 0)
                {
                    throw new InvalidOperationException(
                        $"Scene definition {definition.name} has an invalid build index.");
                }

                if (!_definitions.TryAdd(definition.SceneType, definition))
                {
                    throw new InvalidOperationException(
                        $"Scene catalog contains duplicate {definition.SceneType.Name} definitions.");
                }
            }

            _isInitialized = true;
        }

        public UniTask<Result> LoadAsync<TScene, TSettings>(
            TSettings settings,
            CancellationToken cancellationToken)
            where TScene : IScene<TSettings>
            where TSettings : ISceneSettings
        {
            if (!_definitions.TryGetValue(typeof(TScene), out BaseSceneDefinition definition))
            {
                return UniTask.FromResult(Result.Failure(SceneFlowErrors.DefinitionNotRegistered(typeof(TScene))));
            }

            return LoadAsync(definition, settings, cancellationToken);
        }

        public async UniTask<Result> LoadAsync(
            BaseSceneDefinition sceneDefinition,
            ISceneSettings settings,
            CancellationToken cancellationToken)
        {
            if (!_isInitialized)
            {
                throw new InvalidOperationException("SceneFlowService must be initialized before use.");
            }

            if (sceneDefinition == null)
            {
                throw new ArgumentNullException(nameof(sceneDefinition));
            }

            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            if (!_definitions.TryGetValue(sceneDefinition.SceneType, out BaseSceneDefinition registeredDefinition) ||
                !ReferenceEquals(registeredDefinition, sceneDefinition))
            {
                return Result.Failure(SceneFlowErrors.DefinitionNotRegistered(sceneDefinition.SceneType));
            }

            if (!sceneDefinition.SettingsType.IsInstanceOfType(settings))
            {
                return Result.Failure(SceneFlowErrors.InvalidSettings(
                    sceneDefinition.SettingsType,
                    settings.GetType()));
            }

            if (IsTransitioning)
            {
                return Result.Failure(SceneFlowErrors.TransitionInProgress());
            }

            IsTransitioning = true;

            try
            {
                await ShowTransitionAsync(cancellationToken);
                await ExitCurrentSceneAsync(cancellationToken);
                CancelCurrentScene();

                DiContainer sceneContainer = null;
                var operation = _sceneLoader.LoadSceneAsync(
                    sceneDefinition.BuildIndex,
                    extraBindingsLate: container =>
                    {
                        sceneContainer = container;
                        BindSettings(container, sceneDefinition, settings);
                    });

                await operation.ToUniTask(cancellationToken: cancellationToken);

                if (sceneContainer == null)
                {
                    throw new InvalidOperationException(
                        $"Scene definition {sceneDefinition.name} has no Zenject SceneContext.");
                }

                _currentSceneCancellation =
                    CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                _currentSceneLifecycle = sceneContainer.Resolve<IGameSceneLifecycle>();

                await _currentSceneLifecycle.InitializeAsync(_currentSceneCancellation.Token);

                CurrentSceneDefinition = sceneDefinition;
                return Result.Success();
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                CancelCurrentScene();
                return Result.Failure(SceneFlowErrors.TransitionFailed(exception));
            }
            finally
            {
                IsTransitioning = false;
                await HideTransitionAsync(CancellationToken.None);
            }
        }

        public void Dispose()
        {
            CancelCurrentScene();
        }

        private static void BindSettings(
            DiContainer container,
            BaseSceneDefinition sceneDefinition,
            ISceneSettings settings)
        {
            container.Bind(sceneDefinition.SettingsType).FromInstance(settings).AsSingle();
        }

        private async UniTask ExitCurrentSceneAsync(CancellationToken cancellationToken)
        {
            if (_currentSceneLifecycle != null)
            {
                await _currentSceneLifecycle.ExitAsync(cancellationToken);
            }
        }

        private void CancelCurrentScene()
        {
            _currentSceneCancellation?.Cancel();
            _currentSceneCancellation?.Dispose();
            _currentSceneCancellation = null;
            _currentSceneLifecycle = null;
            CurrentSceneDefinition = null;
        }

        private async UniTask ShowTransitionAsync(CancellationToken cancellationToken)
        {
            foreach (ISceneTransitionPresenter presenter in _transitionPresenters)
            {
                await presenter.ShowAsync(cancellationToken);
            }
        }

        private async UniTask HideTransitionAsync(CancellationToken cancellationToken)
        {
            for (int index = _transitionPresenters.Count - 1; index >= 0; index--)
            {
                await _transitionPresenters[index].HideAsync(cancellationToken);
            }
        }
    }
}
