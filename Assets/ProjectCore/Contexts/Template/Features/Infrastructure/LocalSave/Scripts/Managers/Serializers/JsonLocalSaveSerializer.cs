using System;
using System.Text;
using Domain;
using Newtonsoft.Json;

namespace ProjectCore.Template
{
    public sealed class JsonLocalSaveSerializer : ILocalSaveSerializer
    {
        public LocalSaveSerializerTypes SerializerType => LocalSaveSerializerTypes.Json;

        public Result<byte[]> Serialize<TData>(TData data)
        {
            try
            {
                string json = JsonConvert.SerializeObject(data);
                return Result.Success(Encoding.UTF8.GetBytes(json));
            }
            catch (Exception exception)
            {
                return Result.Failure<byte[]>(LocalSaveErrors.SerializationFailed(exception));
            }
        }

        public Result<TData> Deserialize<TData>(byte[] data)
        {
            if (data == null || data.Length == 0)
            {
                return Result.Failure<TData>(LocalSaveErrors.DeserializationFailed(
                    new JsonSerializationException("Serialized local save data is empty.")));
            }

            try
            {
                TData deserializedData = JsonConvert.DeserializeObject<TData>(Encoding.UTF8.GetString(data));
                if (ReferenceEquals(deserializedData, null))
                {
                    return Result.Failure<TData>(LocalSaveErrors.DeserializationFailed(
                        new JsonSerializationException("Serialized local save data is null.")));
                }

                return Result.Success(deserializedData);
            }
            catch (Exception exception)
            {
                return Result.Failure<TData>(LocalSaveErrors.DeserializationFailed(exception));
            }
        }
    }
}
