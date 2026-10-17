using System.Collections.Generic;
using NUnit.Framework;

namespace ProjectCore.GameCore
{
    public sealed class CombatContactProcessorTests
    {
        [Test]
        public void TargetsHitOncePerExecutionAndAgainAfterExecutionChangeOrReset()
        {
            var processor = new CombatContactProcessor();
            var source = new NetworkEntityIdData(NetworkEntityType.Player, 1);
            var target = new NetworkEntityIdData(NetworkEntityType.Enemy, 1);
            var other = new NetworkEntityIdData(NetworkEntityType.Enemy, 2);
            var contacts = new List<CombatContactData>();
            processor.Process(source, 1, 10, new[] { source, target, target, other }, contacts);
            Assert.That(contacts.Count, Is.EqualTo(2));
            Assert.That(contacts[0].ExecutionId, Is.EqualTo(1));
            contacts.Clear();
            processor.Process(source, 1, 11, new NetworkEntityIdData[0], contacts);
            processor.Process(source, 1, 12, new[] { target }, contacts);
            Assert.That(contacts, Is.Empty, "Leaving and reentering must not repeat a hit.");
            processor.Process(source, 2, 13, new[] { target }, contacts);
            Assert.That(contacts.Count, Is.EqualTo(1));
            contacts.Clear();
            processor.Reset();
            processor.Process(source, 2, 14, new[] { target }, contacts);
            Assert.That(contacts.Count, Is.EqualTo(1));
        }
    }
}
