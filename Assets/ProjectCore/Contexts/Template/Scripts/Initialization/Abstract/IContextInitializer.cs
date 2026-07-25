using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectCore.Template
{
    public interface IContextInitializer
    {
        UniTask InitializeAsync(CancellationToken cancellationToken);
    }
}
