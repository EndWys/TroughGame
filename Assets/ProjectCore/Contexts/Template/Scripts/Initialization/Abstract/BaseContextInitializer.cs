using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace ProjectCore.Template
{
    public abstract class BaseContextInitializer : IContextInitializer
    {
        private readonly IFeatureInitializationFlow _featureInitializationFlow;
        private InitializationState _state;

        protected BaseContextInitializer(IFeatureInitializationFlow featureInitializationFlow)
        {
            _featureInitializationFlow = featureInitializationFlow;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (_state == InitializationState.Initialized)
            {
                return;
            }

            if (_state != InitializationState.NotStarted)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} cannot initialize from state {_state}.");
            }

            _state = InitializationState.Initializing;

            try
            {
                await OnBeforeFeaturesAsync(cancellationToken);
                await _featureInitializationFlow.InitializeFeaturesAsync(cancellationToken);
                await OnAfterFeaturesAsync(cancellationToken);
                _state = InitializationState.Initialized;
            }
            catch
            {
                _state = InitializationState.Failed;
                throw;
            }
        }

        protected virtual UniTask OnBeforeFeaturesAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        protected virtual UniTask OnAfterFeaturesAsync(CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }

        private enum InitializationState
        {
            NotStarted,
            Initializing,
            Initialized,
            Failed
        }
    }
}
