using System;
using System.IO;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain;

namespace ProjectCore.Template
{
    public abstract class BaseFileLocalSaveStorage : ILocalSaveStorage
    {
        private readonly ILocalSavePathProvider _pathProvider;

        protected BaseFileLocalSaveStorage(ILocalSavePathProvider pathProvider)
        {
            _pathProvider = pathProvider ?? throw new ArgumentNullException(nameof(pathProvider));
        }

        public abstract LocalSaveStorageTypes StorageType { get; }

        public async UniTask<Result> SaveAsync(
            string key,
            byte[] data,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (data == null)
            {
                return Result.Failure(LocalSaveErrors.DataIsNull());
            }

            Result<byte[]> protectedData = Protect(data);
            if (protectedData.IsFailure)
            {
                return Result.Failure(protectedData.Errors);
            }

            try
            {
                string path = _pathProvider.GetFilePath(StorageType, key);
                await UniTask.RunOnThreadPool(
                    () => WriteFileAtomically(path, protectedData.Value),
                    cancellationToken: cancellationToken);
                return Result.Success();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                return Result.Failure(LocalSaveErrors.StorageOperationFailed(
                    StorageType,
                    LocalSaveOperations.Save,
                    exception));
            }
        }

        public async UniTask<Result<byte[]>> LoadAsync(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string path = _pathProvider.GetFilePath(StorageType, key);
            if (!File.Exists(path))
            {
                return Result.Failure<byte[]>(LocalSaveErrors.NotFound(key));
            }

            try
            {
                byte[] protectedData = await UniTask.RunOnThreadPool(
                    () => File.ReadAllBytes(path),
                    cancellationToken: cancellationToken);
                return Unprotect(protectedData);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                return Result.Failure<byte[]>(LocalSaveErrors.StorageOperationFailed(
                    StorageType,
                    LocalSaveOperations.Load,
                    exception));
            }
        }

        public UniTask<Result<bool>> ExistsAsync(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            string path = _pathProvider.GetFilePath(StorageType, key);
            return UniTask.FromResult(Result.Success(File.Exists(path)));
        }

        public async UniTask<Result> DeleteAsync(string key, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                string path = _pathProvider.GetFilePath(StorageType, key);
                await UniTask.RunOnThreadPool(
                    () => DeleteFiles(path),
                    cancellationToken: cancellationToken);
                return Result.Success();
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                return Result.Failure(LocalSaveErrors.StorageOperationFailed(
                    StorageType,
                    LocalSaveOperations.Delete,
                    exception));
            }
        }

        protected virtual Result<byte[]> Protect(byte[] data)
        {
            return Result.Success(data);
        }

        protected virtual Result<byte[]> Unprotect(byte[] data)
        {
            return Result.Success(data);
        }

        private static void WriteFileAtomically(string path, byte[] data)
        {
            string directoryPath = Path.GetDirectoryName(path);
            if (string.IsNullOrEmpty(directoryPath))
            {
                throw new InvalidOperationException("Local save file path has no directory.");
            }

            Directory.CreateDirectory(directoryPath);

            string temporaryPath = $"{path}.{Guid.NewGuid():N}.tmp";
            string backupPath = $"{path}.bak";

            try
            {
                using (var stream = new FileStream(
                           temporaryPath,
                           FileMode.Create,
                           FileAccess.Write,
                           FileShare.None))
                {
                    stream.Write(data, 0, data.Length);
                    stream.Flush(true);
                }

                if (File.Exists(path))
                {
                    File.Copy(path, backupPath, true);

                    try
                    {
                        File.Replace(temporaryPath, path, null);
                    }
                    catch (PlatformNotSupportedException)
                    {
                        ReplaceByCopy(temporaryPath, path);
                    }
                    catch (IOException)
                    {
                        ReplaceByCopy(temporaryPath, path);
                    }
                }
                else
                {
                    File.Move(temporaryPath, path);
                }
            }
            finally
            {
                if (File.Exists(temporaryPath))
                {
                    File.Delete(temporaryPath);
                }
            }
        }

        private static void ReplaceByCopy(string temporaryPath, string path)
        {
            File.Copy(temporaryPath, path, true);
            File.Delete(temporaryPath);
        }

        private static void DeleteFiles(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }

            string backupPath = $"{path}.bak";
            if (File.Exists(backupPath))
            {
                File.Delete(backupPath);
            }
        }
    }
}
