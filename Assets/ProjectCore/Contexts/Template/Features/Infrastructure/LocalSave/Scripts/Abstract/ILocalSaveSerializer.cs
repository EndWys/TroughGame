using Domain;

namespace ProjectCore.Template
{
    public interface ILocalSaveSerializer
    {
        LocalSaveSerializerTypes SerializerType { get; }

        Result<byte[]> Serialize<TData>(TData data);

        Result<TData> Deserialize<TData>(byte[] data);
    }
}
