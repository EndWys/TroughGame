using System.Threading;
using Cysharp.Threading.Tasks;

namespace ProjectCore.Template
{
    public sealed class AppTimeFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
            BindAsSingle<IAppTimeService, AppTimeService>();
            BindInterfacesAndSelfAsSingle<LocalTimeProvider>();
            BindInterfacesAndSelfAsSingle<NtpTimeProvider>();
        }

        protected override async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            AppTimeService appTimeService = ResolveAs<IAppTimeService, AppTimeService>();
            LocalTimeProvider localTimeProvider = Resolve<LocalTimeProvider>();
            NtpTimeProvider ntpTimeProvider = Resolve<NtpTimeProvider>();

            appTimeService.AddProvider(localTimeProvider);
            appTimeService.AddProvider(ntpTimeProvider);
            await ntpTimeProvider.InitializeAsync(cancellationToken);
        }
    }
}
