using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace ProjectCore.Template
{
    public abstract class BaseScreenView<TSettings> : BaseScreenView
        where TSettings : IScreenSettings
    {
        protected sealed override UniTask OnInitializeAsync(
            IScreenSettings settings,
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
