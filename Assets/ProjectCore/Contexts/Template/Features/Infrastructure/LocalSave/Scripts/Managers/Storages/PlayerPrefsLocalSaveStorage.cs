using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class PlayerPrefsLocalSaveStorage : ILocalSaveStorage
    {
        public LocalSaveStorageTypes StorageType => LocalSaveStorageTypes.PlayerPrefs;

        public UniTask<Result> SaveAsync(
            string key,
            byte[] data,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (data == null)
            {
                return UniTask.FromResult(Result.Failure(LocalSaveErrors.DataIsNull()));
            }

            try
            {
                PlayerPrefs.SetString(key, Convert.ToBase64String(data));
                PlayerPrefs.Save();
                return UniTask.FromResult(Result.Success());
            }
            catch (Exception exception)
            {
                return UniTask.FromResult(Result.Failure(
                    LocalSaveErrors.StorageOperationFailed(
                        StorageType,
                        LocalSaveOperations.Save,
                        exception)));
            }
        }

        public UniTask<Result<byte[]>> LoadAsync(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!PlayerPrefs.HasKey(key))
            {
                return UniTask.FromResult(Result.Failure<byte[]>(LocalSaveErrors.NotFound(key)));
            }

            try
            {
                return UniTask.FromResult(Result.Success(Convert.FromBase64String(PlayerPrefs.GetString(key))));
            }
            catch (Exception exception)
            {
                return UniTask.FromResult(Result.Failure<byte[]>(
                    LocalSaveErrors.StorageOperationFailed(
                        StorageType,
                        LocalSaveOperations.Load,
                        exception)));
            }
        }

        public UniTask<Result<bool>> ExistsAsync(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return UniTask.FromResult(Result.Success(PlayerPrefs.HasKey(key)));
        }

        public UniTask<Result> DeleteAsync(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
                return UniTask.FromResult(Result.Success());
            }
            catch (Exception exception)
            {
                return UniTask.FromResult(Result.Failure(
                    LocalSaveErrors.StorageOperationFailed(
                        StorageType,
                        LocalSaveOperations.Delete,
                        exception)));
            }
        }
    }
}
