using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectCore.Template
{
    public sealed class CommandLineFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindAsSingle<ICommandLineArgumentsProvider, EnvironmentCommandLineArgumentsProvider>();
            BindInterfacesAsSingle<CommandLineService>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            ResolveAs<ICommandLineService, CommandLineService>().Initialize();
            return UniTask.CompletedTask;
        }
    }
}
