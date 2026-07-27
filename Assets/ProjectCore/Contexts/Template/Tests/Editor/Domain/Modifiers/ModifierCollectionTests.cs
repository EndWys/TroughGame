using System;
using System.Collections.Generic;
using Domain;
using NUnit.Framework;

namespace ProjectCore.Template
{
    public sealed class ModifierCollectionTests
    {
        [Test]
        public void ApplyToRunsModifiersByPriorityAndRegistrationOrder()
        {
            var collection = new ModifierCollection<string>();
            var executionOrder = new List<string>();

            collection.Add(value =>
            {
                executionOrder.Add("priority-10-first");
                return value + "-A";
            }, 10);
            collection.Add(value =>
            {
                executionOrder.Add("priority-0");
                return value + "-B";
            });
            collection.Add(value =>
            {
                executionOrder.Add("priority-10-second");
                return value + "-C";
            }, 10);

            Assert.AreEqual("start-B-A-C", collection.ApplyTo("start"));
            CollectionAssert.AreEqual(
                new[] { "priority-0", "priority-10-first", "priority-10-second" },
                executionOrder);
        }

        [Test]
        public void SubscriptionCanBeDisposedWithoutRemovingAnotherRegistration()
        {
            var collection = new ModifierCollection<int>();
            IDisposable subscription = collection.Add(value => value + 1);
            collection.Add(value => value * 2);

            subscription.Dispose();
            subscription.Dispose();

            Assert.AreEqual(0, collection.ApplyTo(0));
            Assert.AreEqual(2, collection.ApplyTo(1));
        }

        [Test]
        public void RemoveRemovesOnlyFirstMatchingRegistration()
        {
            var collection = new ModifierCollection<int>();
            Func<int, int> modifier = value => value + 1;
            collection.Add(modifier);
            collection.Add(modifier);

            collection.Remove(modifier);

            Assert.AreEqual(1, collection.ApplyTo(0));
        }

    }
}
