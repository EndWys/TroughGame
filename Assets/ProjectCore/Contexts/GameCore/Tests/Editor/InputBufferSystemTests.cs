using System;
using System.Collections.Generic;
using Fusion;
using NUnit.Framework;

namespace ProjectCore.GameCore
{
    public sealed class InputBufferSystemTests
    {
        private InputBufferSystem _system;

        [SetUp]
        public void SetUp()
        {
            _system = new InputBufferSystem();
        }

        [Test]
        public void TryConsumeReturnsOldestValidInput()
        {
            var storage = new TestInputBufferStorage(2);
            InputBufferSettings settings = CreateSettings();

            _system.TryAdd(storage, new TestInputData(1, 10), 1, settings);
            _system.TryAdd(storage, new TestInputData(2, 10), 1, settings);

            bool consumed = _system.TryConsume(storage, 5, out TestInputData input);

            Assert.That(consumed, Is.True);
            Assert.That(input.Value, Is.EqualTo(1));
            Assert.That(storage.Count, Is.EqualTo(1));
        }

        [Test]
        public void TryPeekRemovesExpiredInputs()
        {
            var storage = new TestInputBufferStorage(2);
            InputBufferSettings settings = CreateSettings();
            _system.TryAdd(storage, new TestInputData(1, 3), 1, settings);

            bool found = _system.TryPeek(storage, 4, out _);

            Assert.That(found, Is.False);
            Assert.That(storage.Count, Is.Zero);
        }

        [Test]
        public void InputRemainsValidOnItsExpireTick()
        {
            var storage = new TestInputBufferStorage(1);
            InputBufferSettings settings = CreateSettings();
            _system.TryAdd(storage, new TestInputData(1, 3), 1, settings);

            bool found = _system.TryPeek(storage, 3, out TestInputData input);

            Assert.That(found, Is.True);
            Assert.That(input.Value, Is.EqualTo(1));
        }

        [Test]
        public void TryAddRejectsAlreadyExpiredInput()
        {
            var storage = new TestInputBufferStorage(1);

            bool added = _system.TryAdd(
                storage,
                new TestInputData(1, 2),
                3,
                CreateSettings());

            Assert.That(added, Is.False);
            Assert.That(storage.Count, Is.Zero);
        }

        [Test]
        public void IgnoreNewPreservesExistingInput()
        {
            var storage = new TestInputBufferStorage(2);
            var settings = new InputBufferSettings(
                InputBufferRepeatMode.IgnoreNew,
                InputBufferOverflowMode.RejectNew);
            _system.TryAdd(storage, new TestInputData(1, 10), 1, settings);

            bool added = _system.TryAdd(
                storage,
                new TestInputData(2, 10),
                1,
                settings);

            Assert.That(added, Is.False);
            Assert.That(storage.Count, Is.EqualTo(1));
            Assert.That(storage.Get(0).Value, Is.EqualTo(1));
        }

        [Test]
        public void ReplaceExistingKeepsNewestInput()
        {
            var storage = new TestInputBufferStorage(2);
            var settings = new InputBufferSettings(
                InputBufferRepeatMode.ReplaceExisting,
                InputBufferOverflowMode.RejectNew);
            _system.TryAdd(storage, new TestInputData(1, 10), 1, settings);

            bool added = _system.TryAdd(
                storage,
                new TestInputData(2, 10),
                1,
                settings);

            Assert.That(added, Is.True);
            Assert.That(storage.Count, Is.EqualTo(1));
            Assert.That(storage.Get(0).Value, Is.EqualTo(2));
        }

        [Test]
        public void DropOldestMakesRoomForNewInput()
        {
            var storage = new TestInputBufferStorage(2);
            var settings = new InputBufferSettings(
                InputBufferRepeatMode.Enqueue,
                InputBufferOverflowMode.DropOldest);
            _system.TryAdd(storage, new TestInputData(1, 10), 1, settings);
            _system.TryAdd(storage, new TestInputData(2, 10), 1, settings);

            bool added = _system.TryAdd(
                storage,
                new TestInputData(3, 10),
                1,
                settings);

            Assert.That(added, Is.True);
            Assert.That(storage.Count, Is.EqualTo(2));
            Assert.That(storage.Get(0).Value, Is.EqualTo(2));
            Assert.That(storage.Get(1).Value, Is.EqualTo(3));
        }

        [Test]
        public void RejectNewPreservesFullBuffer()
        {
            var storage = new TestInputBufferStorage(1);
            InputBufferSettings settings = CreateSettings();
            _system.TryAdd(storage, new TestInputData(1, 10), 1, settings);

            bool added = _system.TryAdd(
                storage,
                new TestInputData(2, 10),
                1,
                settings);

            Assert.That(added, Is.False);
            Assert.That(storage.Get(0).Value, Is.EqualTo(1));
        }

        private static InputBufferSettings CreateSettings()
        {
            return new InputBufferSettings(
                InputBufferRepeatMode.Enqueue,
                InputBufferOverflowMode.RejectNew);
        }

        private readonly struct TestInputData : IBufferedInputData
        {
            public TestInputData(int value, Tick expireTick)
            {
                Value = value;
                ExpireTick = expireTick;
            }

            public int Value { get; }
            public int Sequence => Value;
            public Tick ExpireTick { get; }
        }

        private sealed class TestInputBufferStorage :
            IInputBufferStorage<TestInputData>
        {
            private readonly List<TestInputData> _inputs;

            public TestInputBufferStorage(int capacity)
            {
                Capacity = capacity;
                _inputs = new List<TestInputData>(capacity);
            }

            public int Count => _inputs.Count;
            public int Capacity { get; }

            public TestInputData Get(int index)
            {
                return _inputs[index];
            }

            public void Add(TestInputData input)
            {
                _inputs.Add(input);
            }

            public void RemoveAt(int index)
            {
                _inputs.RemoveAt(index);
            }

            public void Clear()
            {
                _inputs.Clear();
            }
        }
    }
}
