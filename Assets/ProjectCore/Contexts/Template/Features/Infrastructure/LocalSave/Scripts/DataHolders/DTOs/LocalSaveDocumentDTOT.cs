using Newtonsoft.Json;

namespace ProjectCore.Template
{
    public sealed class LocalSaveDocumentDTO<TData>
    {
        [JsonConstructor]
        public LocalSaveDocumentDTO(int version, TData data)
        {
            Version = version;
            Data = data;
        }

        public int Version { get; }

        public TData Data { get; }
    }
}
