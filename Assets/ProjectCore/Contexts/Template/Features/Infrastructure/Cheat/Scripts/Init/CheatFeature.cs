using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectCore.Template
{
    public sealed class CheatFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindInterfacesAsSingle<CheatService>();
            BindAsSingle<ICheatArgumentConverterRegistry, CheatArgumentConverterRegistry>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            ResolveAs<ICheatService, CheatService>().Initialize();
            return UniTask.CompletedTask;
        }
    }
}
