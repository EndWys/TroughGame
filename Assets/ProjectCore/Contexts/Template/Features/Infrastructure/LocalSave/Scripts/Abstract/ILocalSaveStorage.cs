using System.Threading;
using Cysharp.Threading.Tasks;
using Domain;

namespace ProjectCore.Template
{
    public interface ILocalSaveStorage
    {
        LocalSaveStorageTypes StorageType { get; }

        UniTask<Result> SaveAsync(
            string key,
            byte[] data,
            CancellationToken cancellationToken);

        UniTask<Result<byte[]>> LoadAsync(string key, CancellationToken cancellationToken);

        UniTask<Result<bool>> ExistsAsync(string key, CancellationToken cancellationToken);

        UniTask<Result> DeleteAsync(string key, CancellationToken cancellationToken);
    }
}
