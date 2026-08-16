using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace ProjectCore.Template
{
    public abstract class BaseLoadingScreenView<TSettings> : BaseLoadingScreenView
        where TSettings : ILoadingScreenSettings
    {
        protected sealed override UniTask OnInitializeAsync(
            ILoadingScreenSettings settings,
            CancellationToken cancellationToken)
        {
            if (settings is not TSettings typedSettings)
            {
                throw new ArgumentException(
                    $"{GetType().Name} requires {typeof(TSettings).Name}.",
                    nameof(settings));
            }

            return OnInitializeAsync(typedSettings, cancellationToken);
        }

        protected abstract UniTask OnInitializeAsync(
            TSettings settings,
            CancellationToken cancellationToken);
    }
}
