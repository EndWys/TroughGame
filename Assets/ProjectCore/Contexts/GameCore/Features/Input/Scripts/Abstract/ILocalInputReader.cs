namespace ProjectCore.GameCore
{
    public interface ILocalInputReader
    {
        void Capture();

        T ReadValue<T>(string actionPath) where T : struct;

        LocalButtonStateData ReadButton(string actionPath);
    }
}
