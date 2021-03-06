﻿using ConcurrentPriorityQueue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConcurrentPriorityQueueTests.FunctionalTests
{
    [TestClass]
    public class ConcurrentPriorityQueueTests : AbstractPriorityQueueTests
    {
        protected override IPriorityQueue<TElement, TPriority> CreateQueue<TElement, TPriority>(int capacity)
        {
            return new ConcurrentPriorityQueue<TElement, TPriority>(capacity);
        }

        [TestMethod]
        public void EnqueueAfterExceedingCapacity()
        {
            var target = new ConcurrentPriorityQueue<string, int>(3);

            Assert.AreEqual(0, target.Count);

            target.Enqueue("a", 1);
            target.Enqueue("b", 2);
            target.Enqueue("c", 3);

            target.Enqueue("d", 4);
            Assert.AreEqual(4, target.Count);
            Assert.AreEqual(6, target.Capacity);
            string result = string.Join(",", target);
            Assert.AreEqual("d,c,b,a", result);

            target.Enqueue("e", 0);
            Assert.AreEqual(5, target.Count);
            Assert.AreEqual(6, target.Capacity);
            result = string.Join(",", target);
            Assert.AreEqual("d,c,b,a,e", result);
        }

        [TestMethod]
        public void DequeueWithTooBigCapacity()
        {
            var target = new ConcurrentPriorityQueue<string, int>(50);

            for (var i = 0; i < 13; i++)
            {
                target.Enqueue("a", i);    
            }
            Assert.AreEqual(50, target.Capacity);
            Assert.AreEqual(13, target.Count);
            target.Dequeue();
            Assert.AreEqual(25, target.Capacity);
            Assert.AreEqual(12, target.Count);
            target.Dequeue();
            Assert.AreEqual(25, target.Capacity);
            Assert.AreEqual(11, target.Count);
        }

        [TestMethod]
        public void EnqueueDequeue()
        {
            var target = new ConcurrentPriorityQueue<string, int>(7);

            target.Enqueue("a", 7);
            target.Enqueue("b", 6);
            target.Enqueue("c", 5);
            Assert.AreEqual("a", target.Dequeue());
            target.Enqueue("d", 4);
            Assert.AreEqual("b", target.Dequeue());
            target.Enqueue("a", 7);
            Assert.AreEqual("a", target.Dequeue());
            Assert.AreEqual("c", target.Dequeue());
            Assert.AreEqual("d", target.Dequeue());
            Assert.AreEqual(0, target.Count);
        }

        [TestMethod]
        public void Trim()
        {
            var target = new ConcurrentPriorityQueue<string, int>(2);
            int expectedCapacity = target.Capacity;
            for (var i = 0; i < 10; i++)
            {
                if (target.Count == target.Capacity) expectedCapacity = expectedCapacity * ConcurrentPriorityQueue<string, int>._resizeFactor;
                target.Enqueue("a", i);
            }
            Assert.AreEqual(expectedCapacity, target.Capacity);
            string items = string.Join(",", target);
            target.Trim();
            Assert.AreEqual(10, target.Capacity);
            Assert.AreEqual(items, string.Join(",", target));
        }

        [TestMethod]
        public void EqualsForObjects()
        {
            var target = new ConcurrentPriorityQueue<string, int>(4);

            Assert.IsTrue(target.Equals("a", "a"));
            Assert.IsFalse(target.Equals("a", "b"));
            Assert.IsFalse(target.Equals("a", null));
            Assert.IsFalse(target.Equals(null, "a"));
            Assert.IsTrue(target.Equals(null, null));
        }

        [TestMethod]
        public void EqualsForValueTypes()
        {
            var target = new ConcurrentPriorityQueue<int, int>(4);

            Assert.IsTrue(target.Equals(1, 1));
            Assert.IsFalse(target.Equals(1, 2));
            Assert.IsFalse(target.Equals(2, 1));
        }
    }
}
