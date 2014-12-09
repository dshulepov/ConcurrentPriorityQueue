using System;
using System.Collections;
using System.Linq;
using ConcurrentPriorityQueue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConcurrentPriorityQueueTests.FunctionalTests
{
    public abstract class AbstractPriorityQueueTests
    {
        protected abstract IPriorityQueue<TElement, TPriority> CreateQueue<TElement, TPriority>(int capacity) where TPriority : IComparable<TPriority>;

        [TestMethod]
        public void Initialize()
        {
            var target = CreateQueue<string, int>(5);

            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(5, target.Capacity);

            AssertEx.Throws<ArgumentOutOfRangeException>(() => target = CreateQueue<string, int>(0));
        }

        [TestMethod]
        public void Enqueue()
        {
            var target = CreateQueue<string, int>(4);

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
            var target = CreateQueue<string, int>(4);
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
            var target = CreateQueue<string, int>(4);

            AssertEx.Throws<InvalidOperationException>(() => target.Peek());

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
            var target = CreateQueue<string, int>(6);
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
        public void UpdatePriority()
        {
            var target = CreateQueue<string, int>(4);
            target.Enqueue("a", 1);
            target.Enqueue("b", 2);
            target.Enqueue("c", 3);
            target.Enqueue("d", 4);

            Assert.AreEqual("d", target.Peek());
            Assert.AreEqual("d,c,b,a", string.Join(",", target));

            target.UpdatePriority("a", 5);
            Assert.AreEqual("a", target.Peek());
            Assert.AreEqual("a,d,c,b", string.Join(",", target));

            target.UpdatePriority("d", 1);
            Assert.AreEqual("a", target.Peek());
            Assert.AreEqual("a,c,b,d", string.Join(",", target));

            AssertEx.Throws<ArgumentException>(() => target.UpdatePriority("x", 23));
        }

        [TestMethod]
        public void Clear()
        {
            var target = CreateQueue<string, int>(7);

            target.Enqueue("a", 7);
            target.Enqueue("b", 6);
            target.Enqueue("c", 5);

            target.Clear();
            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(0, target.ToArray().Length);
        }


        [TestMethod]
        public void Contains()
        {
            var target = CreateQueue<string, int>(5);
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
    }
}