using System;
using System.Collections;
using System.Linq;
using ConcurrentPriorityQueue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConcurrentPriorityQueueTests.FunctionalTests
{
    [TestClass]
    public class ConcurrentPriorityQueueTests
    {
        [TestMethod]
        public void Initialize()
        {
            var target = new ConcurrentPriorityQueue<string, int>(5);

            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(5, target.Capacity);

            AssertEx.Throws<ArgumentOutOfRangeException>(() => target = new ConcurrentPriorityQueue<string, int>(0));
        }

        [TestMethod]
        public void Enqueue()
        {
            var target = new ConcurrentPriorityQueue<string, int>(4);

            Assert.AreEqual(0, target.Count);

            target.Enqueue("a", 1);
            Assert.AreEqual(1, target.Count);

            target.Enqueue("b", 2);
            Assert.AreEqual(2, target.Count);

            target.Enqueue("c", 3);
            Assert.AreEqual(3, target.Count);

            target.Enqueue("d", 4);
            Assert.AreEqual(4, target.Count);
        }

        [TestMethod]
        public void Dequeue()
        {
            var target = new ConcurrentPriorityQueue<string, int>(4);
            target.Enqueue("a", 1);
            target.Enqueue("b", 4);
            target.Enqueue("c", 3);
            target.Enqueue("d", 2);

            Assert.AreEqual(4, target.Count);
            Assert.AreEqual("b", target.Dequeue());
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual("c", target.Dequeue());
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual("d", target.Dequeue());
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual("a", target.Dequeue());
            Assert.AreEqual(0, target.Count);

            AssertEx.Throws<InvalidOperationException>(() => target.Dequeue());
        }

        [TestMethod]
        public void Peek()
        {
            var target = new ConcurrentPriorityQueue<string, int>(4);
            target.Enqueue("a", 1);

            Assert.AreEqual("a", target.Peek());
            Assert.AreEqual(1, target.Count);

            target.Enqueue("b", 4);

            Assert.AreEqual("b", target.Peek());
            Assert.AreEqual(2, target.Count);

            target.Enqueue("c", 3);

            Assert.AreEqual("b", target.Peek());
            Assert.AreEqual(3, target.Count);

            target.Enqueue("d", 2);

            Assert.AreEqual("b", target.Peek());
            Assert.AreEqual(4, target.Count);

            target.Dequeue();

            Assert.AreEqual("c", target.Peek());
            Assert.AreEqual(3, target.Count);
        }

        [TestMethod]
        public void GetEnumerator()
        {
            var target = new ConcurrentPriorityQueue<string, int>(6);
            target.Enqueue("a", 1);
            target.Enqueue("b", 2);
            target.Enqueue("c", 3);
            target.Enqueue("d", 4);
            target.Enqueue("e", 5);

            var enumerator1 = target.GetEnumerator();
            var enumerator2 = ((IEnumerable)target).GetEnumerator();

            Assert.AreNotEqual(enumerator1, enumerator2);

            string result = string.Join(",", target);
            Assert.AreEqual("e,d,c,b,a", result);
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
        public void Clear()
        {
            var target = new ConcurrentPriorityQueue<string, int>(7);

            target.Enqueue("a", 7);
            target.Enqueue("b", 6);
            target.Enqueue("c", 5);

            target.Clear();
            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(0, target.ToArray().Length);
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
        public void Contains()
        {
            var target = new ConcurrentPriorityQueue<string, int>(5);
            target.Enqueue("a", 1);
            target.Enqueue("b", 1);
            target.Enqueue("c", 1);
            target.Enqueue("a", 1);
            target.Enqueue(null, 4);

            Assert.IsTrue(target.Contains("a"));
            Assert.IsTrue(target.Contains("b"));
            Assert.IsTrue(target.Contains("c"));
            Assert.IsFalse(target.Contains("d"));
            Assert.IsTrue(target.Contains(null));
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
