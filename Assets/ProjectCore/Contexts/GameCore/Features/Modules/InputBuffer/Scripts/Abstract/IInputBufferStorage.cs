namespace ProjectCore.GameCore
{
    public interface IInputBufferStorage<T> where T : struct, IBufferedInputData
    {
        int Count { get; }
        int Capacity { get; }

        T Get(int index);
        void Add(T input);
        void RemoveAt(int index);
        void Clear();
    }
}
