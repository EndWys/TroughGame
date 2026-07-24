using Cysharp.Threading.Tasks;
using ProjectCore.Project;
using System.Threading;
using UnityEngine;

namespace ProjectCore.Preloader
{
    public sealed class ApplicationInitializationFlow : IApplicationInitializationFlow
    {
        private readonly IProjectContextInitializer _projectContextInitializer;
        private readonly IPreloaderContextInitializer _preloaderContextInitializer;
        private readonly IApplicationFlowCoordinator _applicationFlowCoordinator;

        public ApplicationInitializationFlow(
            IProjectContextInitializer projectContextInitializer,
            IPreloaderContextInitializer preloaderContextInitializer,
            IApplicationFlowCoordinator applicationFlowCoordinator)
        {
            _projectContextInitializer = projectContextInitializer;
            _preloaderContextInitializer = preloaderContextInitializer;
            _applicationFlowCoordinator = applicationFlowCoordinator;
        }

        public async UniTask RunAsync(
            string initialSceneName,
            CancellationToken applicationCancellationToken,
            CancellationToken preloaderCancellationToken)
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            await _projectContextInitializer.InitializeAsync(
                applicationCancellationToken);

            await _preloaderContextInitializer.InitializeAsync(
                preloaderCancellationToken);

            await _applicationFlowCoordinator.LoadSceneAsync(
                initialSceneName,
                applicationCancellationToken);
        }
    }
}
