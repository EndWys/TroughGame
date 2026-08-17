using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProjectCore.Template
{
    public sealed class LocalSaveDescriptor<TData>
    {
        public LocalSaveDescriptor(
            string key,
            LocalSaveStorageTypes storageType,
            LocalSaveSerializerTypes serializerType = LocalSaveSerializerTypes.Json,
            int version = 1,
            params string[] formerKeys)
        {
            Key = key;
            StorageType = storageType;
            SerializerType = serializerType;
            Version = version;

            string[] copiedFormerKeys = formerKeys ?? Array.Empty<string>();
            FormerKeys = new ReadOnlyCollection<string>((string[])copiedFormerKeys.Clone());
        }

        public string Key { get; }

        public LocalSaveStorageTypes StorageType { get; }

        public LocalSaveSerializerTypes SerializerType { get; }

        public int Version { get; }

        public IReadOnlyList<string> FormerKeys { get; }
    }
}
