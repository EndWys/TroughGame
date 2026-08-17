using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using Zenject;

namespace ProjectCore.Template
{
    public abstract class BaseFeatureGroup : IBaseFeature
    {
        private readonly List<IBaseFeature> _features = new();
        private GroupState _state;

        public void InstallBindings(DiContainer container)
        {
            if (_state != GroupState.NotInstalled)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} cannot install from state {_state}.");
            }

            AddFeatures();

            foreach (IBaseFeature feature in _features)
            {
                feature.InstallBindings(container);
            }

            _state = GroupState.Installed;
        }

        public async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            if (_state == GroupState.Initialized)
            {
                return;
            }

            if (_state != GroupState.Installed)
            {
                throw new InvalidOperationException(
                    $"{GetType().Name} cannot initialize from state {_state}.");
            }

            _state = GroupState.Initializing;

            try
            {
                foreach (IBaseFeature feature in _features)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await feature.InitializeAsync(cancellationToken);
                }

                _state = GroupState.Initialized;
            }
            catch
            {
                _state = GroupState.Failed;
                throw;
            }
        }

        protected abstract void AddFeatures();

        protected void AddFeature<T>() where T : IBaseFeature, new()
        {
            _features.Add(new T());
        }

        private enum GroupState
        {
            NotInstalled,
            Installed,
            Initializing,
            Initialized,
            Failed
        }
    }
}
