namespace ProjectCore.Template
{
    public interface ILogService
    {
        void AddLogHandler(ILogHandler logHandler);

        void RemoveLogHandler(ILogHandler logHandler);
    }
}
