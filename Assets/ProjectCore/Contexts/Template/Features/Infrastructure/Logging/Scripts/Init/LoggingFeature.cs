using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectCore.Template
{
    public sealed class LoggingFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindInterfacesAndSelfAsSingle<LogService>();
            BindAsSingle<IDebugLogger, DebugLoggerService>();
        }

        protected override UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            ResolveAs<ILogService, LogService>().Initialize();
            return UniTask.CompletedTask;
        }
    }
}
