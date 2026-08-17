using System;
using System.Collections.Generic;
using Domain;

namespace ProjectCore.Template
{
    public static class LocalSaveDescriptorValidation
    {
        public static Result Validate<TData>(LocalSaveDescriptor<TData> descriptor)
        {
            if (descriptor == null)
            {
                return Result.Failure(LocalSaveErrors.InvalidDefinition(
                    "Local save descriptor cannot be null."));
            }

            if (!IsValidKey(descriptor.Key))
            {
                return Result.Failure(LocalSaveErrors.InvalidDefinition(
                    "Local save keys must contain only letters, digits, '.', '-', or '_'."));
            }

            if (!Enum.IsDefined(typeof(LocalSaveStorageTypes), descriptor.StorageType))
            {
                return Result.Failure(LocalSaveErrors.InvalidDefinition(
                    $"Unsupported local save storage type '{descriptor.StorageType}'."));
            }

            if (!Enum.IsDefined(typeof(LocalSaveSerializerTypes), descriptor.SerializerType))
            {
                return Result.Failure(LocalSaveErrors.InvalidDefinition(
                    $"Unsupported local save serializer type '{descriptor.SerializerType}'."));
            }

            if (descriptor.Version < 1)
            {
                return Result.Failure(LocalSaveErrors.InvalidDefinition(
                    "Local save version must be greater than zero."));
            }

            var keys = new HashSet<string>(StringComparer.Ordinal) { descriptor.Key };
            for (int i = 0; i < descriptor.FormerKeys.Count; i++)
            {
                string formerKey = descriptor.FormerKeys[i];
                if (!IsValidKey(formerKey) || !keys.Add(formerKey))
                {
                    return Result.Failure(LocalSaveErrors.InvalidDefinition(
                        "Former local save keys must be valid and unique."));
                }
            }

            return Result.Success();
        }

        private static bool IsValidKey(string key)
        {
            if (string.IsNullOrWhiteSpace(key) || key.Length > 128)
            {
                return false;
            }

            for (int i = 0; i < key.Length; i++)
            {
                char character = key[i];
                bool isAllowed = char.IsLetterOrDigit(character) || character == '.' ||
                    character == '-' || character == '_';
                if (!isAllowed)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
