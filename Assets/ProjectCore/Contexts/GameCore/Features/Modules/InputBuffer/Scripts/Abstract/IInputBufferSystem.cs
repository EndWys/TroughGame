using Fusion;

namespace ProjectCore.GameCore
{
    public interface IInputBufferSystem
    {
        bool TryAdd<T>(
            IInputBufferStorage<T> storage,
            T input,
            Tick currentTick,
            InputBufferSettings settings)
            where T : struct, IBufferedInputData;

        bool TryPeek<T>(
            IInputBufferStorage<T> storage,
            Tick currentTick,
            out T input)
            where T : struct, IBufferedInputData;

        bool TryConsume<T>(
            IInputBufferStorage<T> storage,
            Tick currentTick,
            out T input)
            where T : struct, IBufferedInputData;

        int RemoveExpired<T>(
            IInputBufferStorage<T> storage,
            Tick currentTick)
            where T : struct, IBufferedInputData;
    }
}
