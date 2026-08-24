using System;
using Fusion;

namespace ProjectCore.GameCore
{
    public sealed class NetworkInputBufferStorageAdapter<T> : IInputBufferStorage<T>
        where T : struct, IBufferedInputData, INetworkStruct
    {
        private readonly NetworkLinkedList<T> _storage;

        public NetworkInputBufferStorageAdapter(NetworkLinkedList<T> storage)
        {
            _storage = storage;
        }

        public int Count => _storage.Count;
        public int Capacity => _storage.Capacity;

        public T Get(int index)
        {
            ValidateIndex(index);
            return _storage.Get(index);
        }

        public void Add(T input)
        {
            if (Count >= Capacity)
            {
                throw new InvalidOperationException(
                    "Network input buffer storage is full.");
            }

            for (int i = 0; i < Count; i++)
            {
                if (_storage.Get(i).Sequence == input.Sequence)
                {
                    throw new InvalidOperationException(
                        $"Buffered input sequence '{input.Sequence}' is duplicated.");
                }
            }

            _storage.Add(input);
        }

        public void RemoveAt(int index)
        {
            ValidateIndex(index);
            _storage.Remove(_storage.Get(index));
        }

        public void Clear()
        {
            _storage.Clear();
        }

        private void ValidateIndex(int index)
        {
            if ((uint)index >= (uint)Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
        }
    }
}
