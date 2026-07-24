using Cysharp.Threading.Tasks;
using System;
using UnityEngine;
using Zenject;

namespace ProjectCore.Preloader
{
    public sealed class ApplicationEntryPoint : MonoBehaviour
    {
        [SerializeField] private string _initialSceneName = "Scene_Prototype";

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
                return;
            }

            _isStarted = true;

            _applicationInitializationFlow.RunAsync(
                    _initialSceneName,
                    Application.exitCancellationToken,
                    destroyCancellationToken)
                .Forget(HandleInitializationException);
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
