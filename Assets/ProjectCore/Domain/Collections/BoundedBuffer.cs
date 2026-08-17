using System;
using System.Collections;
using System.Collections.Generic;

namespace Domain
{
    internal sealed class BoundedBuffer<T> : IReadOnlyList<T>
    {
        private T[] _items;
        private T[] _scratch;
        private int _head;

        public BoundedBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _items = new T[capacity];
            _scratch = new T[capacity];
        }

        public int Count { get; private set; }

        public T this[int index]
        {
            get
            {
                if ((uint)index >= (uint)Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _items[ToPhysicalIndex(index)];
            }
        }

        public void Add(T item)
        {
            if (Count < _items.Length)
            {
                _items[ToPhysicalIndex(Count)] = item;
                Count++;
                return;
            }

            _items[_head] = item;
            _head = (_head + 1) % _items.Length;
        }

        public int RemoveAll(Predicate<T> predicate)
        {
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            int keptCount = 0;
            for (int i = 0; i < Count; i++)
            {
                T item = this[i];
                if (!predicate(item))
                    _scratch[keptCount++] = item;
            }

            int removedCount = Count - keptCount;
            if (removedCount == 0)
                return 0;

            Array.Clear(_items, 0, _items.Length);
            (_items, _scratch) = (_scratch, _items);
            _head = 0;
            Count = keptCount;
            return removedCount;
        }

        public void Clear()
        {
            Array.Clear(_items, 0, _items.Length);
            Array.Clear(_scratch, 0, _scratch.Length);
            _head = 0;
            Count = 0;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return this[i];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private int ToPhysicalIndex(int logicalIndex)
        {
            return (_head + logicalIndex) % _items.Length;
        }
    }
}
