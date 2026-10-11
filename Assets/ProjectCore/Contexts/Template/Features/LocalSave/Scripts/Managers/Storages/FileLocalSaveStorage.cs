namespace ProjectCore.Template
{
    public sealed class FileLocalSaveStorage : BaseFileLocalSaveStorage
    {
        public FileLocalSaveStorage(ILocalSavePathProvider pathProvider)
            : base(pathProvider)
        {
        }

        public override LocalSaveStorageTypes StorageType => LocalSaveStorageTypes.File;
    }
}
