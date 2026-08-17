using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectCore.Template
{
    public interface IFeatureInitializationFlow
    {
        UniTask InitializeFeaturesAsync(CancellationToken cancellationToken);
    }
}
