using Cysharp.Threading.Tasks;
using Domain;
using ProjectCore.Project;
using ProjectCore.Template;
using System;
using System.Threading;
using UnityEngine;

namespace ProjectCore.Preloader
{
    public sealed class ApplicationInitializationFlow : IApplicationInitializationFlow
    {
        private readonly IProjectContextInitializer _projectContextInitializer;
        private readonly IPreloaderContextInitializer _preloaderContextInitializer;
        private readonly ISceneFlowService _sceneFlowService;

        public ApplicationInitializationFlow(
            IProjectContextInitializer projectContextInitializer,
            IPreloaderContextInitializer preloaderContextInitializer,
            ISceneFlowService sceneFlowService)
        {
            _projectContextInitializer = projectContextInitializer;
            _preloaderContextInitializer = preloaderContextInitializer;
            _sceneFlowService = sceneFlowService;
        }

        public async UniTask RunAsync(
            BaseSceneDefinition initialSceneDefinition,
            CancellationToken applicationCancellationToken,
            CancellationToken preloaderCancellationToken)
        {
            Application.targetFrameRate = 60;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            await _projectContextInitializer.InitializeAsync(
                applicationCancellationToken);

            await _preloaderContextInitializer.InitializeAsync(
                preloaderCancellationToken);

            Result result = await _sceneFlowService.LoadAsync(
                initialSceneDefinition,
                EmptyScenePayload.Instance,
                applicationCancellationToken);

            if (result.IsFailure)
            {
                throw new InvalidOperationException(result.FirstError.Message);
            }
        }
    }
}
