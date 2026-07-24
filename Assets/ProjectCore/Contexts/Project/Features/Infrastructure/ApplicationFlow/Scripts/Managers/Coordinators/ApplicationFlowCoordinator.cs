using Cysharp.Threading.Tasks;
using ProjectCore.Template;
using System;
using System.Threading;
using UnityEngine.SceneManagement;
using Zenject;

namespace ProjectCore.Project
{
    public sealed class ApplicationFlowCoordinator : IApplicationFlowCoordinator, IDisposable
    {
        private readonly ZenjectSceneLoader _sceneLoader;

        private CancellationTokenSource _sceneCancellation;
        private bool _isLoading;

        public ApplicationFlowCoordinator(ZenjectSceneLoader sceneLoader)
        {
            _sceneLoader = sceneLoader;
        }

        public string CurrentSceneName { get; private set; }
        public bool IsSceneReady { get; private set; }

        public async UniTask LoadSceneAsync(
            string sceneName,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                throw new ArgumentException("A scene name is required.", nameof(sceneName));
            }

            if (_isLoading)
            {
                throw new InvalidOperationException("A scene transition is already running.");
            }

            _isLoading = true;
            IsSceneReady = false;
            CancelCurrentScene();

            DiContainer sceneContainer = null;

            try
            {
                var operation = _sceneLoader.LoadSceneAsync(
                    sceneName,
                    LoadSceneMode.Single,
                    extraBindingsLate: container => sceneContainer = container);

                await operation.ToUniTask(cancellationToken: cancellationToken);

                if (sceneContainer == null)
                {
                    throw new InvalidOperationException(
                        $"Scene '{sceneName}' did not provide a Zenject SceneContext.");
                }

                _sceneCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

                ISceneInitializer sceneInitializer = sceneContainer.Resolve<ISceneInitializer>();
                
                await sceneInitializer.InitializeAsync(_sceneCancellation.Token);

                CurrentSceneName = sceneName;
                IsSceneReady = true;
            }
            catch
            {
                CancelCurrentScene();
                throw;
            }
            finally
            {
                _isLoading = false;
            }
        }

        public void Dispose()
        {
            CancelCurrentScene();
        }

        private void CancelCurrentScene()
        {
            if (_sceneCancellation == null)
            {
                return;
            }

            _sceneCancellation.Cancel();
            _sceneCancellation.Dispose();
            _sceneCancellation = null;
            CurrentSceneName = null;
            IsSceneReady = false;
        }
    }
}
