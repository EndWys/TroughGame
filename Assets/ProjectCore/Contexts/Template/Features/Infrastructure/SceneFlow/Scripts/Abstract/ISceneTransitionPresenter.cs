using Cysharp.Threading.Tasks;
using System.Threading;

namespace ProjectCore.Template
{
    public interface ISceneTransitionPresenter
    {
        UniTask ShowAsync(CancellationToken cancellationToken);
        UniTask HideAsync(CancellationToken cancellationToken);
    }
}
