namespace ProjectCore.Template
{
    public interface ILocalSavePathProvider
    {
        string GetFilePath(LocalSaveStorageTypes storageType, string key);
    }
}
