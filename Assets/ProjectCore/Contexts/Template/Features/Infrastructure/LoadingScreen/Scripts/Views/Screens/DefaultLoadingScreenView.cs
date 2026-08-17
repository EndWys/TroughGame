using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectCore.Template
{
    public sealed class DefaultLoadingScreenView
        : BaseLoadingScreenView<EmptyLoadingScreenSettings>
    {
        protected override UniTask OnInitializeAsync(
            EmptyLoadingScreenSettings settings,
            CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
