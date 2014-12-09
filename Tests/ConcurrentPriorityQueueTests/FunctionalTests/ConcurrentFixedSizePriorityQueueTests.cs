using ConcurrentPriorityQueue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConcurrentPriorityQueueTests.FunctionalTests
{
    [TestClass]
    public class ConcurrentFixedSizePriorityQueueTests : AbstractPriorityQueueTests
    {
        protected override IPriorityQueue<TElement, TPriority> CreateQueue<TElement, TPriority>(int capacity)
        {
            return new ConcurrentFixedSizePriorityQueue<TElement, TPriority>(capacity);
        }

        [TestMethod]
        public void EnqueueAfterExceedingCapacity()
        {
            var target = new ConcurrentFixedSizePriorityQueue<string, int>(3);

            Assert.AreEqual(0, target.Count);

            target.Enqueue("a", 1);
            target.Enqueue("b", 2);
            target.Enqueue("c", 3);

            target.Enqueue("d", 4);
            Assert.AreEqual(3, target.Count);
            string result = string.Join(",", target);
            Assert.AreEqual("d,b,a", result);

            target.Enqueue("e", 0);
            Assert.AreEqual(3, target.Count);
            result = string.Join(",", target);
            Assert.AreEqual("b,a,e", result);
        }

        [TestMethod]
        public void EnqueueDequeue()
        {
            var target = new ConcurrentFixedSizePriorityQueue<string, int>(7);

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
        public void EqualsForObjects()
        {
            var target = new ConcurrentFixedSizePriorityQueue<string, int>(4);

            Assert.IsTrue(target.Equals("a", "a"));
            Assert.IsFalse(target.Equals("a", "b"));
            Assert.IsFalse(target.Equals("a", null));
            Assert.IsFalse(target.Equals(null, "a"));
            Assert.IsTrue(target.Equals(null, null));
        }

        [TestMethod]
        public void EqualsForValueTypes()
        {
            var target = new ConcurrentFixedSizePriorityQueue<int, int>(4);

            Assert.IsTrue(target.Equals(1, 1));
            Assert.IsFalse(target.Equals(1, 2));
            Assert.IsFalse(target.Equals(2, 1));
        }
    }
}
