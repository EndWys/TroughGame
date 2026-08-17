using Cysharp.Threading.Tasks;
using Domain;
using ProjectCore.Template;
using System;
using System.Threading;

namespace ProjectCore.Prototype
{
    public sealed class PrototypeFeature : BaseFeature
    {
        protected override void InstallBindings()
        {
        }

        protected override async UniTask InitializeAsync(CancellationToken cancellationToken)
        {
            Result result = await Resolve<IScreenNavigationSystem>()
                .OpenRootAsync<PrototypeGameHUDScreenView, EmptyScreenSettings>(
                    new EmptyScreenSettings(),
                    cancellationToken);

            if (result.IsFailure)
            {
                throw new InvalidOperationException(result.FirstError.Message);
            }
        }
    }
}
