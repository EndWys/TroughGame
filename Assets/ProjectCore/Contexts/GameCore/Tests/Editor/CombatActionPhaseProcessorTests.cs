using System;
using NUnit.Framework;
using UnityEngine;

namespace ProjectCore.GameCore
{
    public sealed class CombatActionPhaseProcessorTests
    {
        private readonly CombatActionPhaseProcessor _processor = new CombatActionPhaseProcessor();
        private readonly CombatActionTimingData _timing = new CombatActionTimingData(2, 3, 5, 1, 4);

        [TestCase(-1, CombatActionPhase.None, false, false)]
        [TestCase(0, CombatActionPhase.Preparation, false, false)]
        [TestCase(1, CombatActionPhase.Preparation, false, false)]
        [TestCase(2, CombatActionPhase.Active, false, false)]
        [TestCase(4, CombatActionPhase.Active, false, false)]
        [TestCase(5, CombatActionPhase.Recovery, false, false)]
        [TestCase(6, CombatActionPhase.Recovery, true, false)]
        [TestCase(8, CombatActionPhase.Recovery, true, false)]
        [TestCase(9, CombatActionPhase.Recovery, false, false)]
        [TestCase(10, CombatActionPhase.None, false, true)]
        [TestCase(11, CombatActionPhase.None, false, true)]
        public void EvaluateUsesPhaseAndHalfOpenWindowBoundaries(
            int elapsed,
            CombatActionPhase phase,
            bool canChain,
            bool complete)
        {
            var state = new CombatActionStateModel();
            _processor.Start(ref state, 300, new Vector2(3f, 4f), 100);

            CombatActionSnapshotData snapshot = _processor.Evaluate(
                state, CombatActionKind.Attack, _timing, 100 + elapsed);

            Assert.That(snapshot.ActionId, Is.EqualTo(300));
            Assert.That(snapshot.ExecutionId, Is.EqualTo(1));
            Assert.That(snapshot.Kind, Is.EqualTo(CombatActionKind.Attack));
            Assert.That(snapshot.Direction.x, Is.EqualTo(0.6f).Within(0.00001f));
            Assert.That(snapshot.Direction.y, Is.EqualTo(0.8f).Within(0.00001f));
            Assert.That(snapshot.Phase, Is.EqualTo(phase));
            Assert.That(snapshot.CanStartNextAction, Is.EqualTo(canChain));
            Assert.That(snapshot.IsComplete, Is.EqualTo(complete));
        }

        [Test]
        public void TickWrapAndReplayFromCopiedStateProduceTheSameSnapshot()
        {
            var state = new CombatActionStateModel();
            int startedTick = int.MaxValue - 3;
            _processor.Start(ref state, ushort.MaxValue, Vector2.left, startedTick);
            CombatActionStateModel copy = state;
            int currentTick = unchecked(startedTick + 7);

            CombatActionSnapshotData forward = _processor.Evaluate(
                state, CombatActionKind.Dodge, _timing, currentTick);
            _processor.Evaluate(state, CombatActionKind.Dodge, _timing, unchecked(startedTick + 10));
            CombatActionSnapshotData replay = new CombatActionPhaseProcessor().Evaluate(
                copy, CombatActionKind.Dodge, _timing, currentTick);

            Assert.That(replay, Is.EqualTo(forward));
            Assert.That(replay.Phase, Is.EqualTo(CombatActionPhase.Recovery));
            Assert.That(replay.CanStartNextAction, Is.True);
            Assert.That(state, Is.EqualTo(copy));
        }

        [Test]
        public void CancelPreservesExecutionCounterAndNextStartSkipsZero()
        {
            var state = new CombatActionStateModel { ExecutionId = uint.MaxValue - 1 };
            _processor.Start(ref state, 1, Vector2.up, 15);
            Assert.That(state.ExecutionId, Is.EqualTo(uint.MaxValue));

            _processor.Cancel(ref state);

            Assert.That(state.ExecutionId, Is.EqualTo(uint.MaxValue));
            Assert.That(state.ActionId, Is.Zero);
            Assert.That(state.StartedTick, Is.Zero);
            Assert.That(state.Direction, Is.EqualTo(Vector2.zero));
            CombatActionSnapshotData idle = _processor.Evaluate(state, CombatActionKind.Dodge, _timing, 30);
            Assert.That(idle.Phase, Is.EqualTo(CombatActionPhase.None));
            Assert.That(idle.IsComplete, Is.False);
            Assert.That(idle.CanStartNextAction, Is.False);
            Assert.That(idle.ExecutionId, Is.EqualTo(uint.MaxValue));

            _processor.Start(ref state, 2, Vector2.right, 31);
            Assert.That(state.ExecutionId, Is.EqualTo(1));
        }

        [Test]
        public void EqualWindowBoundsDisableChainingThroughoutRecovery()
        {
            var state = new CombatActionStateModel();
            _processor.Start(ref state, 1, Vector2.zero, 0);
            var timing = new CombatActionTimingData(2, 3, 5, 2, 2);

            for (int tick = 5; tick < 10; tick++)
            {
                Assert.That(_processor.Evaluate(state, CombatActionKind.Attack, timing, tick).CanStartNextAction,
                    Is.False);
            }
        }

        [Test]
        public void InvalidStartDoesNotReplaceAnExistingExecution()
        {
            var state = new CombatActionStateModel();
            _processor.Start(ref state, 1, Vector2.right, 10);
            CombatActionStateModel original = state;

            Assert.Throws<ArgumentOutOfRangeException>(() => _processor.Start(ref state, 0, Vector2.up, 20));
            Assert.That(state, Is.EqualTo(original));
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _processor.Start(ref state, 2, new Vector2(float.NaN, 0f), 20));
            Assert.That(state, Is.EqualTo(original));
        }
    }
}
