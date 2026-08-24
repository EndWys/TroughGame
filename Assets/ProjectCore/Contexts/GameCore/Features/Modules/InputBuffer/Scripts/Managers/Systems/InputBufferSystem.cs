using System;
using Fusion;

namespace ProjectCore.GameCore
{
    public sealed class InputBufferSystem : IInputBufferSystem
    {
        public bool TryAdd<T>(
            IInputBufferStorage<T> storage,
            T input,
            Tick currentTick,
            InputBufferSettings settings)
            where T : struct, IBufferedInputData
        {
            ValidateStorage(storage);
            RemoveExpired(storage, currentTick);

            if (currentTick > input.ExpireTick)
            {
                return false;
            }

            if (storage.Count > 0)
            {
                switch (settings.RepeatMode)
                {
                    case InputBufferRepeatMode.IgnoreNew:
                        return false;
                    case InputBufferRepeatMode.ReplaceExisting:
                        storage.Clear();
                        break;
                    case InputBufferRepeatMode.Enqueue:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(settings),
                            settings.RepeatMode,
                            "Unsupported input buffer repeat mode.");
                }
            }

            if (storage.Count >= storage.Capacity)
            {
                switch (settings.OverflowMode)
                {
                    case InputBufferOverflowMode.RejectNew:
                        return false;
                    case InputBufferOverflowMode.DropOldest:
                        storage.RemoveAt(0);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(settings),
                            settings.OverflowMode,
                            "Unsupported input buffer overflow mode.");
                }
            }

            storage.Add(input);
            return true;
        }

        public bool TryPeek<T>(
            IInputBufferStorage<T> storage,
            Tick currentTick,
            out T input)
            where T : struct, IBufferedInputData
        {
            ValidateStorage(storage);
            RemoveExpired(storage, currentTick);

            if (storage.Count == 0)
            {
                input = default;
                return false;
            }

            input = storage.Get(0);
            return true;
        }

        public bool TryConsume<T>(
            IInputBufferStorage<T> storage,
            Tick currentTick,
            out T input)
            where T : struct, IBufferedInputData
        {
            if (!TryPeek(storage, currentTick, out input))
            {
                return false;
            }

            storage.RemoveAt(0);
            return true;
        }

        public int RemoveExpired<T>(
            IInputBufferStorage<T> storage,
            Tick currentTick)
            where T : struct, IBufferedInputData
        {
            ValidateStorage(storage);

            int removedCount = 0;
            for (int i = storage.Count - 1; i >= 0; i--)
            {
                if (currentTick <= storage.Get(i).ExpireTick)
                {
                    continue;
                }

                storage.RemoveAt(i);
                removedCount++;
            }

            return removedCount;
        }

        private static void ValidateStorage<T>(IInputBufferStorage<T> storage)
            where T : struct, IBufferedInputData
        {
            if (storage == null)
            {
                throw new ArgumentNullException(nameof(storage));
            }

            if (storage.Capacity <= 0)
            {
                throw new InvalidOperationException(
                    "Input buffer storage capacity must be positive.");
            }
        }
    }
}
