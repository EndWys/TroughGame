using System;
using NUnit.Framework;

namespace ProjectCore.GameCore
{
    public sealed class ActionTimelineTests
    {
        private readonly ActionTimingData _timing = new ActionTimingData(2, 3, 5, 1, 4);

        [TestCase(-1, ActionPhase.None, false, false)]
        [TestCase(0, ActionPhase.Preparation, false, false)]
        [TestCase(1, ActionPhase.Preparation, false, false)]
        [TestCase(2, ActionPhase.Active, false, false)]
        [TestCase(4, ActionPhase.Active, false, false)]
        [TestCase(5, ActionPhase.Recovery, false, false)]
        [TestCase(6, ActionPhase.Recovery, false, true)]
        [TestCase(8, ActionPhase.Recovery, false, true)]
        [TestCase(9, ActionPhase.Recovery, false, false)]
        [TestCase(10, ActionPhase.None, true, false)]
        [TestCase(11, ActionPhase.None, true, false)]
        public void BothNetworkAnchorsUseTheSamePhaseAndWindowBoundaries(
            int elapsed, ActionPhase phase, bool isComplete, bool isWindowOpen)
        {
            foreach (int startedTick in new[] { 100, int.MaxValue - 3, int.MinValue })
            {
                int currentTick = unchecked(startedTick + elapsed);
                ActionTimelineData fromStart = ActionTimelineData.FromStartTick(_timing, startedTick);
                ActionTimelineData fromEnd = ActionTimelineData.FromEndTick(_timing, unchecked(startedTick + 10));
                ActionPhaseSnapshotData snapshot = fromStart.Evaluate(currentTick);

                Assert.That(snapshot.Phase, Is.EqualTo(phase));
                Assert.That(snapshot.IsComplete, Is.EqualTo(isComplete));
                Assert.That(snapshot.IsRecoveryCancelWindowOpen, Is.EqualTo(isWindowOpen));
                Assert.That(fromEnd.Evaluate(currentTick), Is.EqualTo(snapshot));
                Assert.That(fromEnd, Is.EqualTo(fromStart));
            }
        }

        [TestCase(100)]
        [TestCase(int.MaxValue - 3)]
        [TestCase(int.MinValue)]
        public void PhaseAndWindowDeadlinesAreDerivedFromTheExecution(int startedTick)
        {
            ActionTimelineData timeline = ActionTimelineData.FromStartTick(_timing, startedTick);

            Assert.That(timeline.StartedTick, Is.EqualTo(startedTick));
            Assert.That(timeline.ActiveStartTick, Is.EqualTo(unchecked(startedTick + 2)));
            Assert.That(timeline.ActiveEndTick, Is.EqualTo(unchecked(startedTick + 5)));
            Assert.That(timeline.RecoveryStartTick, Is.EqualTo(timeline.ActiveEndTick));
            Assert.That(timeline.EndTick, Is.EqualTo(unchecked(startedTick + 10)));
            Assert.That(timeline.RecoveryCancelWindowStartTick, Is.EqualTo(unchecked(startedTick + 6)));
            Assert.That(timeline.RecoveryCancelWindowEndTick, Is.EqualTo(unchecked(startedTick + 9)));
        }

        [Test]
        public void MissingNetworkAnchorHasNoPhaseDeadlineOrCompletion()
        {
            foreach (ActionTimelineData timeline in new[]
            {
                default(ActionTimelineData),
                ActionTimelineData.FromStartTick(_timing, null),
                ActionTimelineData.FromEndTick(_timing, null),
            })
            {
                Assert.That(timeline.StartedTick, Is.Null);
                Assert.That(timeline.ActiveStartTick, Is.Null);
                Assert.That(timeline.ActiveEndTick, Is.Null);
                Assert.That(timeline.RecoveryStartTick, Is.Null);
                Assert.That(timeline.EndTick, Is.Null);
                Assert.That(timeline.RecoveryCancelWindowStartTick, Is.Null);
                Assert.That(timeline.RecoveryCancelWindowEndTick, Is.Null);
                Assert.That(timeline.Evaluate(100), Is.EqualTo(default(ActionPhaseSnapshotData)));
            }
        }

        [Test]
        public void DisabledWindowNeverOpensButActionStillCompletes()
        {
            var timing = new ActionTimingData(2, 3, 5);
            ActionTimelineData timeline = ActionTimelineData.FromStartTick(timing, 100);

            Assert.That(timeline.RecoveryCancelWindowStartTick, Is.Null);
            Assert.That(timeline.RecoveryCancelWindowEndTick, Is.Null);
            for (int tick = 100; tick <= 110; tick++)
                Assert.That(timeline.Evaluate(tick).IsRecoveryCancelWindowOpen, Is.False);
            Assert.That(timeline.Evaluate(109).IsComplete, Is.False);
            Assert.That(timeline.Evaluate(110).IsComplete, Is.True);
        }

        [Test]
        public void ReplayingAnEarlierTickDoesNotRestartOrAdvanceTheTimeline()
        {
            ActionTimelineData timeline = ActionTimelineData.FromEndTick(_timing, 110);
            ActionPhaseSnapshotData before = timeline.Evaluate(106);
            timeline.Evaluate(110);

            Assert.That(timeline.Evaluate(106), Is.EqualTo(before));
            Assert.That(ActionTimelineData.FromEndTick(_timing, 110).Evaluate(106), Is.EqualTo(before));
        }

        [TestCase(0, 3, 5, 1, 4)]
        [TestCase(2, 0, 5, 1, 4)]
        [TestCase(2, 3, 0, 0, 0)]
        [TestCase(2, 3, 5, -1, 4)]
        [TestCase(2, 3, 5, 4, 1)]
        [TestCase(2, 3, 5, 1, 6)]
        [TestCase(int.MaxValue, 1, 1, 0, 0)]
        public void InvalidTimingsCannotCreateAnExecutionSchedule(
            int preparation, int active, int recovery, int windowStart, int windowEnd)
        {
            Assert.That(() => new ActionTimingData(preparation, active, recovery, windowStart, windowEnd),
                Throws.InstanceOf<ArgumentException>());
        }
    }
}
