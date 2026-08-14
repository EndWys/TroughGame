using Cysharp.Threading.Tasks;
using ProjectCore.Template;
using System.Threading;

namespace ProjectCore.Prototype
{
    public sealed class PrototypeGameHUDScreenView : BaseScreenView<EmptyScreenSettings>
    {
        protected override UniTask OnInitializeAsync(
            EmptyScreenSettings settings,
            CancellationToken cancellationToken)
        {
            return UniTask.CompletedTask;
        }
    }
}
