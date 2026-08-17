using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Domain;
using UnityEngine;

namespace ProjectCore.Template
{
    public sealed class SecureFileLocalSaveStorage : BaseFileLocalSaveStorage
    {
        private const byte EnvelopeVersion = 1;
        private const int InitializationVectorSize = 16;
        private const int AuthenticationTagSize = 32;

        private readonly byte[] _authenticationKey;
        private readonly byte[] _encryptionKey;

        public SecureFileLocalSaveStorage(ILocalSavePathProvider pathProvider)
            : base(pathProvider)
        {
            byte[] masterKey = CreateMasterKey();

            _encryptionKey = DeriveKey(masterKey, "encryption");
            _authenticationKey = DeriveKey(masterKey, "authentication");
        }

        public override LocalSaveStorageTypes StorageType => LocalSaveStorageTypes.SecureFile;

        protected override Result<byte[]> Protect(byte[] data)
        {
            if (data == null)
            {
                return Result.Failure<byte[]>(LocalSaveErrors.DataIsNull());
            }

            try
            {
                byte[] initializationVector = CreateInitializationVector();
                byte[] encryptedData = Encrypt(data, initializationVector);
                byte[] authenticatedData = Combine(
                    new[] { EnvelopeVersion },
                    initializationVector,
                    encryptedData);
                byte[] authenticationTag = ComputeAuthenticationTag(authenticatedData);

                return Result.Success(Combine(authenticatedData, authenticationTag));
            }
            catch (Exception exception)
            {
                return Result.Failure<byte[]>(LocalSaveErrors.ProtectedDataInvalid(exception));
            }
        }

        protected override Result<byte[]> Unprotect(byte[] data)
        {
            if (data == null || data.Length < 1 + InitializationVectorSize + AuthenticationTagSize)
            {
                return Result.Failure<byte[]>(LocalSaveErrors.ProtectedDataInvalid(
                    new InvalidDataException("Secure local save envelope is incomplete.")));
            }

            try
            {
                if (data[0] != EnvelopeVersion)
                {
                    throw new InvalidDataException("Secure local save envelope version is unsupported.");
                }

                int encryptedDataOffset = 1 + InitializationVectorSize;
                int encryptedDataLength = data.Length - encryptedDataOffset - AuthenticationTagSize;
                if (encryptedDataLength <= 0)
                {
                    throw new InvalidDataException("Secure local save envelope has no encrypted data.");
                }

                byte[] authenticatedData = Slice(data, 0, encryptedDataOffset + encryptedDataLength);
                byte[] storedAuthenticationTag = Slice(
                    data,
                    encryptedDataOffset + encryptedDataLength,
                    AuthenticationTagSize);
                byte[] computedAuthenticationTag = ComputeAuthenticationTag(authenticatedData);

                if (!AreEqualInConstantTime(storedAuthenticationTag, computedAuthenticationTag))
                {
                    throw new CryptographicException("Secure local save authentication failed.");
                }

                byte[] initializationVector = Slice(data, 1, InitializationVectorSize);
                byte[] encryptedData = Slice(data, encryptedDataOffset, encryptedDataLength);
                return Result.Success(Decrypt(encryptedData, initializationVector));
            }
            catch (Exception exception)
            {
                return Result.Failure<byte[]>(LocalSaveErrors.ProtectedDataInvalid(exception));
            }
        }

        private static byte[] DeriveKey(byte[] masterKey, string purpose)
        {
            using var hmac = new HMACSHA256(masterKey);
            return hmac.ComputeHash(Encoding.UTF8.GetBytes($"TroughGame.LocalSave.{purpose}"));
        }

        private static byte[] CreateMasterKey()
        {
            string source = $"{Application.identifier}:{SystemInfo.deviceUniqueIdentifier}";
            byte[] sourceBytes = Encoding.UTF8.GetBytes(source);

            using SHA256 sha256 = SHA256.Create();
            return sha256.ComputeHash(sourceBytes);
        }

        private static byte[] CreateInitializationVector()
        {
            var initializationVector = new byte[InitializationVectorSize];
            using RandomNumberGenerator randomNumberGenerator = RandomNumberGenerator.Create();
            randomNumberGenerator.GetBytes(initializationVector);
            return initializationVector;
        }

        private byte[] Encrypt(byte[] data, byte[] initializationVector)
        {
            using Aes aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.IV = initializationVector;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform encryptor = aes.CreateEncryptor();
            return encryptor.TransformFinalBlock(data, 0, data.Length);
        }

        private byte[] Decrypt(byte[] data, byte[] initializationVector)
        {
            using Aes aes = Aes.Create();
            aes.Key = _encryptionKey;
            aes.IV = initializationVector;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            using ICryptoTransform decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(data, 0, data.Length);
        }

        private byte[] ComputeAuthenticationTag(byte[] data)
        {
            using var hmac = new HMACSHA256(_authenticationKey);
            return hmac.ComputeHash(data);
        }

        private static byte[] Combine(params byte[][] arrays)
        {
            int length = 0;
            for (int i = 0; i < arrays.Length; i++)
            {
                length += arrays[i].Length;
            }

            var combined = new byte[length];
            int offset = 0;

            for (int i = 0; i < arrays.Length; i++)
            {
                byte[] array = arrays[i];
                Buffer.BlockCopy(array, 0, combined, offset, array.Length);
                offset += array.Length;
            }

            return combined;
        }

        private static byte[] Slice(byte[] source, int offset, int length)
        {
            var slice = new byte[length];
            Buffer.BlockCopy(source, offset, slice, 0, length);
            return slice;
        }

        private static bool AreEqualInConstantTime(byte[] left, byte[] right)
        {
            if (left.Length != right.Length)
            {
                return false;
            }

            int difference = 0;
            for (int i = 0; i < left.Length; i++)
            {
                difference |= left[i] ^ right[i];
            }

            return difference == 0;
        }
    }
}
