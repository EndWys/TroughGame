using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Zenject;

namespace ProjectCore.Template
{
    public sealed class FeatureInitializationFlow : IFeatureInitializationFlow
    {
        private readonly IReadOnlyList<IBaseFeature> _features;
        private readonly DiContainer _container;
        private InitializationState _state;

        public FeatureInitializationFlow(List<IBaseFeature> features, DiContainer container)
        {
            _features = features;
            _container = container;
        }

        public async UniTask InitializeFeaturesAsync(CancellationToken cancellationToken)
        {
            if (_state == InitializationState.Initialized)
            {
                return;
            }

            if (_state != InitializationState.NotStarted)
            {
                throw new InvalidOperationException(
                    $"Feature initialization cannot start from state {_state}.");
            }

            _state = InitializationState.Initializing;

            try
            {
                foreach (IBaseFeature feature in _features)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await feature.InitializeAsync(_container, cancellationToken);
                }

                _state = InitializationState.Initialized;
            }
            catch
            {
                _state = InitializationState.Failed;
                throw;
            }
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
