using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Domain;

namespace ProjectCore.Template
{
    public sealed class LocalSaveService : ILocalSaveService, IDisposable
    {
        private const string KeyPrefix = "ProjectCore.LocalSave.";

        private readonly SemaphoreSlim _operationSemaphore = new SemaphoreSlim(1, 1);
        private readonly IReadOnlyDictionary<LocalSaveStorageTypes, ILocalSaveStorage> _storages;
        private readonly IReadOnlyDictionary<LocalSaveSerializerTypes, ILocalSaveSerializer> _serializers;

        public LocalSaveService(
            IEnumerable<ILocalSaveStorage> storages,
            IEnumerable<ILocalSaveSerializer> serializers)
        {
            if (storages == null)
            {
                throw new ArgumentNullException(nameof(storages));
            }

            if (serializers == null)
            {
                throw new ArgumentNullException(nameof(serializers));
            }

            _storages = storages.ToDictionary(storage => storage.StorageType);
            _serializers = serializers.ToDictionary(serializer => serializer.SerializerType);
        }

        public async UniTask<Result> SaveAsync<TData>(
            LocalSaveDescriptor<TData> descriptor,
            TData data,
            CancellationToken cancellationToken)
        {
            await _operationSemaphore.WaitAsync(cancellationToken);

            try
            {
                return await SaveAsyncInternal(descriptor, data, cancellationToken);
            }
            finally
            {
                _operationSemaphore.Release();
            }
        }

        public async UniTask<Result<TData>> LoadAsync<TData>(
            LocalSaveDescriptor<TData> descriptor,
            CancellationToken cancellationToken)
        {
            await _operationSemaphore.WaitAsync(cancellationToken);

            try
            {
                return await LoadAsyncInternal(descriptor, cancellationToken);
            }
            finally
            {
                _operationSemaphore.Release();
            }
        }

        public async UniTask<Result<TData>> LoadOrCreateAsync<TData>(
            LocalSaveDescriptor<TData> descriptor,
            Func<TData> createData,
            CancellationToken cancellationToken)
        {
            if (createData == null)
            {
                throw new ArgumentNullException(nameof(createData));
            }

            await _operationSemaphore.WaitAsync(cancellationToken);

            try
            {
                Result<TData> loadedData = await LoadAsyncInternal(descriptor, cancellationToken);
                if (loadedData.IsSuccess || !LocalSaveErrors.IsNotFound(loadedData.FirstError))
                {
                    return loadedData;
                }

                cancellationToken.ThrowIfCancellationRequested();
                TData createdData = createData();
                Result savedData = await SaveAsyncInternal(descriptor, createdData, cancellationToken);
                return savedData.IsSuccess
                    ? Result.Success(createdData)
                    : Result.Failure<TData>(savedData.Errors);
            }
            finally
            {
                _operationSemaphore.Release();
            }
        }

        public async UniTask<Result<bool>> ExistsAsync<TData>(
            LocalSaveDescriptor<TData> descriptor,
            CancellationToken cancellationToken)
        {
            await _operationSemaphore.WaitAsync(cancellationToken);

            try
            {
                Result descriptorValidation = ValidateDescriptor(descriptor);
                if (descriptorValidation.IsFailure)
                {
                    return Result.Failure<bool>(descriptorValidation.Errors);
                }

                Result<LocalSaveDependencies> dependenciesResult = GetDependencies(descriptor);
                if (dependenciesResult.IsFailure)
                {
                    return Result.Failure<bool>(dependenciesResult.Errors);
                }

                LocalSaveDependencies dependencies = dependenciesResult.Value;
                foreach (string key in GetKeys(descriptor))
                {
                    Result<bool> exists = await dependencies.Storage.ExistsAsync(
                        GetStorageKey(key),
                        cancellationToken);
                    if (exists.IsFailure)
                    {
                        return exists;
                    }

                    if (exists.Value)
                    {
                        return Result.Success(true);
                    }
                }

                return Result.Success(false);
            }
            finally
            {
                _operationSemaphore.Release();
            }
        }

        public async UniTask<Result> DeleteAsync<TData>(
            LocalSaveDescriptor<TData> descriptor,
            CancellationToken cancellationToken)
        {
            await _operationSemaphore.WaitAsync(cancellationToken);

            try
            {
                Result descriptorValidation = ValidateDescriptor(descriptor);
                if (descriptorValidation.IsFailure)
                {
                    return Result.Failure(descriptorValidation.Errors);
                }

                Result<LocalSaveDependencies> dependenciesResult = GetDependencies(descriptor);
                if (dependenciesResult.IsFailure)
                {
                    return Result.Failure(dependenciesResult.Errors);
                }

                LocalSaveDependencies dependencies = dependenciesResult.Value;
                foreach (string key in GetKeys(descriptor))
                {
                    Result deleted = await dependencies.Storage.DeleteAsync(
                        GetStorageKey(key),
                        cancellationToken);
                    if (deleted.IsFailure)
                    {
                        return deleted;
                    }
                }

                return Result.Success();
            }
            finally
            {
                _operationSemaphore.Release();
            }
        }

        public void Dispose()
        {
            _operationSemaphore.Dispose();
        }

        private async UniTask<Result> SaveAsyncInternal<TData>(
            LocalSaveDescriptor<TData> descriptor,
            TData data,
            CancellationToken cancellationToken)
        {
            if (ReferenceEquals(data, null))
            {
                return Result.Failure(LocalSaveErrors.DataIsNull());
            }

            Result descriptorValidation = ValidateDescriptor(descriptor);
            if (descriptorValidation.IsFailure)
            {
                return descriptorValidation;
            }

            Result<LocalSaveDependencies> dependenciesResult = GetDependencies(descriptor);
            if (dependenciesResult.IsFailure)
            {
                return Result.Failure(dependenciesResult.Errors);
            }

            LocalSaveDependencies dependencies = dependenciesResult.Value;
            var document = new LocalSaveDocumentDTO<TData>(descriptor.Version, data);
            Result<byte[]> serializedData = dependencies.Serializer.Serialize(document);
            if (serializedData.IsFailure)
            {
                return Result.Failure(serializedData.Errors);
            }

            return await dependencies.Storage.SaveAsync(
                GetStorageKey(descriptor.Key),
                serializedData.Value,
                cancellationToken);
        }

        private async UniTask<Result<TData>> LoadAsyncInternal<TData>(
            LocalSaveDescriptor<TData> descriptor,
            CancellationToken cancellationToken)
        {
            Result descriptorValidation = ValidateDescriptor(descriptor);
            if (descriptorValidation.IsFailure)
            {
                return Result.Failure<TData>(descriptorValidation.Errors);
            }

            Result<LocalSaveDependencies> dependenciesResult = GetDependencies(descriptor);
            if (dependenciesResult.IsFailure)
            {
                return Result.Failure<TData>(dependenciesResult.Errors);
            }

            LocalSaveDependencies dependencies = dependenciesResult.Value;
            foreach (string key in GetKeys(descriptor))
            {
                Result<TData> loadedData = await LoadByKeyAsync(
                    descriptor,
                    dependencies,
                    key,
                    cancellationToken);
                if (loadedData.IsSuccess)
                {
                    if (key != descriptor.Key)
                    {
                        Result migratedSave = await SaveAsyncInternal(
                            descriptor,
                            loadedData.Value,
                            cancellationToken);
                        if (migratedSave.IsFailure)
                        {
                            return Result.Failure<TData>(migratedSave.Errors);
                        }

                        Result migratedDelete = await dependencies.Storage.DeleteAsync(
                            GetStorageKey(key),
                            cancellationToken);
                        if (migratedDelete.IsFailure)
                        {
                            return Result.Failure<TData>(migratedDelete.Errors);
                        }
                    }

                    return loadedData;
                }

                if (!LocalSaveErrors.IsNotFound(loadedData.FirstError))
                {
                    return loadedData;
                }
            }

            return Result.Failure<TData>(LocalSaveErrors.NotFound(descriptor.Key));
        }

        private async UniTask<Result<TData>> LoadByKeyAsync<TData>(
            LocalSaveDescriptor<TData> descriptor,
            LocalSaveDependencies dependencies,
            string key,
            CancellationToken cancellationToken)
        {
            Result<byte[]> storedData = await dependencies.Storage.LoadAsync(
                GetStorageKey(key),
                cancellationToken);
            if (storedData.IsFailure)
            {
                return Result.Failure<TData>(storedData.Errors);
            }

            Result<LocalSaveDocumentDTO<TData>> documentResult =
                dependencies.Serializer.Deserialize<LocalSaveDocumentDTO<TData>>(storedData.Value);
            if (documentResult.IsFailure)
            {
                return Result.Failure<TData>(documentResult.Errors);
            }

            LocalSaveDocumentDTO<TData> document = documentResult.Value;
            if (document == null)
            {
                return Result.Failure<TData>(LocalSaveErrors.DeserializationFailed(
                    new InvalidOperationException("Local save document is null.")));
            }

            if (document.Version != descriptor.Version)
            {
                return Result.Failure<TData>(LocalSaveErrors.VersionMismatch(
                    document.Version,
                    descriptor.Version));
            }

            if (ReferenceEquals(document.Data, null))
            {
                return Result.Failure<TData>(LocalSaveErrors.DataIsNull());
            }

            return Result.Success(document.Data);
        }

        private static Result ValidateDescriptor<TData>(LocalSaveDescriptor<TData> descriptor)
        {
            return LocalSaveDescriptorValidation.Validate(descriptor);
        }

        private Result<LocalSaveDependencies> GetDependencies<TData>(LocalSaveDescriptor<TData> descriptor)
        {
            if (!_storages.TryGetValue(descriptor.StorageType, out ILocalSaveStorage storage))
            {
                return Result.Failure<LocalSaveDependencies>(
                    LocalSaveErrors.StorageNotRegistered(descriptor.StorageType));
            }

            if (!_serializers.TryGetValue(descriptor.SerializerType, out ILocalSaveSerializer serializer))
            {
                return Result.Failure<LocalSaveDependencies>(
                    LocalSaveErrors.SerializerNotRegistered(descriptor.SerializerType));
            }

            return Result.Success(new LocalSaveDependencies(storage, serializer));
        }

        private static IEnumerable<string> GetKeys<TData>(LocalSaveDescriptor<TData> descriptor)
        {
            yield return descriptor.Key;

            for (int i = 0; i < descriptor.FormerKeys.Count; i++)
            {
                yield return descriptor.FormerKeys[i];
            }
        }

        private static string GetStorageKey(string key)
        {
            return $"{KeyPrefix}{key}";
        }

        private sealed class LocalSaveDependencies
        {
            public LocalSaveDependencies(
                ILocalSaveStorage storage,
                ILocalSaveSerializer serializer)
            {
                Storage = storage;
                Serializer = serializer;
            }

            public ILocalSaveStorage Storage { get; }

            public ILocalSaveSerializer Serializer { get; }
        }
    }
}
