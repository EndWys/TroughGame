using Cysharp.Threading.Tasks;
using ProjectCore.Template;
using System;
using System.Threading;
using UnityEngine;
using Zenject;

namespace ProjectCore.Preloader
{
    public sealed class ApplicationEntryPoint : MonoBehaviour
    {
        [SerializeField] private BaseSceneDefinition _initialSceneDefinition;

        private IApplicationInitializationFlow _applicationInitializationFlow;
        private bool _isStarted;

        [Inject]
        private void Construct(IApplicationInitializationFlow applicationInitializationFlow)
        {
            _applicationInitializationFlow = applicationInitializationFlow;
        }

        private void Start()
        {
            if (_isStarted)
            {
                throw new InvalidOperationException(
                    "ApplicationEntryPoint can start only once.");
            }

            _isStarted = true;

            RunInitializationAsync()
                .Forget(HandleInitializationException);
        }

        private async UniTask RunInitializationAsync()
        {
            using var linkedPreloaderCancellation =
                CancellationTokenSource.CreateLinkedTokenSource(
                    Application.exitCancellationToken,
                    destroyCancellationToken);

            await _applicationInitializationFlow.RunAsync(
                _initialSceneDefinition,
                Application.exitCancellationToken,
                linkedPreloaderCancellation.Token);
        }

        private static void HandleInitializationException(Exception exception)
        {
            if (exception is OperationCanceledException &&
                Application.exitCancellationToken.IsCancellationRequested)
            {
                return;
            }

            Debug.LogException(exception);
        }
    }
}
