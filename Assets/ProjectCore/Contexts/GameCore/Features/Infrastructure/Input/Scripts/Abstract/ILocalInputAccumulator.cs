namespace ProjectCore.GameCore
{
    public interface ILocalInputAccumulator
    {
        T ReadValue<T>(string actionPath) where T : struct;

        LocalButtonInputData ConsumeButton(string actionPath);
    }
}
