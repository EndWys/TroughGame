using System;
using Domain;

namespace ProjectCore.Template
{
    public static class LocalSaveErrors
    {
        public static Error InvalidDefinition(string message)
        {
            return new Error("LocalSave.InvalidDefinition", message);
        }

        public static Error DataIsNull()
        {
            return new Error("LocalSave.DataIsNull", "Local save data cannot be null.");
        }

        public static Error NotFound(string key)
        {
            return new Error("LocalSave.NotFound", $"Local save '{key}' was not found.");
        }

        public static Error StorageNotRegistered(LocalSaveStorageTypes storageType)
        {
            return new Error(
                "LocalSave.StorageNotRegistered",
                $"The {storageType} local save storage is not registered.");
        }

        public static Error SerializerNotRegistered(LocalSaveSerializerTypes serializerType)
        {
            return new Error(
                "LocalSave.SerializerNotRegistered",
                $"The {serializerType} local save serializer is not registered.");
        }

        public static Error SerializationFailed(Exception exception)
        {
            return new Error(
                "LocalSave.SerializationFailed",
                "Local save data could not be serialized.",
                exception.ToString());
        }

        public static Error DeserializationFailed(Exception exception)
        {
            return new Error(
                "LocalSave.DeserializationFailed",
                "Local save data could not be deserialized.",
                exception.ToString());
        }

        public static Error StorageOperationFailed(
            LocalSaveStorageTypes storageType,
            LocalSaveOperations operation,
            Exception exception)
        {
            return new Error(
                "LocalSave.StorageOperationFailed",
                $"The {storageType} local save storage failed to {operation} data.",
                exception.ToString());
        }

        public static Error ProtectedDataInvalid(Exception exception)
        {
            return new Error(
                "LocalSave.ProtectedDataInvalid",
                "Secure local save data is invalid or has been modified.",
                exception.ToString());
        }

        public static Error VersionMismatch(int savedVersion, int expectedVersion)
        {
            return new Error(
                "LocalSave.VersionMismatch",
                $"Local save version {savedVersion} is incompatible with expected " +
                $"version {expectedVersion}.");
        }

        public static bool IsNotFound(Error error)
        {
            return error.Code == "LocalSave.NotFound";
        }
    }
}
