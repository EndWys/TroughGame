using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain;

namespace ProjectCore.Template
{
    public interface ILocalSaveService
    {
        UniTask<Result> SaveAsync<TData>(
            LocalSaveDescriptor<TData> descriptor,
            TData data,
            CancellationToken cancellationToken);

        UniTask<Result<TData>> LoadAsync<TData>(
            LocalSaveDescriptor<TData> descriptor,
            CancellationToken cancellationToken);

        UniTask<Result<TData>> LoadOrCreateAsync<TData>(
            LocalSaveDescriptor<TData> descriptor,
            Func<TData> createData,
            CancellationToken cancellationToken);

        UniTask<Result<bool>> ExistsAsync<TData>(
            LocalSaveDescriptor<TData> descriptor,
            CancellationToken cancellationToken);

        UniTask<Result> DeleteAsync<TData>(
            LocalSaveDescriptor<TData> descriptor,
            CancellationToken cancellationToken);
    }
}
