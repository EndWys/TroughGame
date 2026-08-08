using System;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class LocalSavePathProvider : ILocalSavePathProvider
    {
        private readonly string _rootPath;

        public LocalSavePathProvider()
            : this(Application.persistentDataPath)
        {
        }

        public LocalSavePathProvider(string rootPath)
        {
            _rootPath = string.IsNullOrWhiteSpace(rootPath)
                ? throw new ArgumentException("Local save root path cannot be empty.", nameof(rootPath))
                : rootPath;
        }

        public string GetFilePath(LocalSaveStorageTypes storageType, string key)
        {
            string fileName = GetFileName(key);
            return System.IO.Path.Combine(_rootPath, "LocalSave", storageType.ToString(), fileName);
        }

        private static string GetFileName(string key)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] hash = sha256.ComputeHash(keyBytes);
            var builder = new StringBuilder(hash.Length * 2);

            for (int i = 0; i < hash.Length; i++)
            {
                builder.Append(hash[i].ToString("x2"));
            }

            return builder.Append(".save").ToString();
        }
    }
}
