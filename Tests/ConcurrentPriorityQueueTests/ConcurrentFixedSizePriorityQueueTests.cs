using ConcurrentPriorityQueue;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentPriorityQueueTests
{
    [TestFixture]
    public class ConcurrentFixedSizePriorityQueueTests
    {
        [Test]
        public void Initialize()
        {
            var target = new ConcurrentFixedSizePriorityQueue<string, int>(5);

            Assert.AreEqual(0, target.Count);
            Assert.AreEqual(5, target.Capacity);

            Assert.Throws<ArgumentOutOfRangeException>(() => target = new ConcurrentFixedSizePriorityQueue<string, int>(0));
        }

        [Test]
        public void Enqueue()
        {
            var target = new ConcurrentFixedSizePriorityQueue<string, int>(4);

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

        [Test]
        public void Dequeue()
        {
            var target = new ConcurrentFixedSizePriorityQueue<string, int>(4);
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

            Assert.Throws<InvalidOperationException>(() => target.Dequeue());
        }

        [Test]
        public void GetEnumerator()
        {
            var target = new ConcurrentFixedSizePriorityQueue<string, int>(6);
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

        [Test]
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

        [Test]
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

        [Test]
        public void SingleThreadTiming()
        {
            const int Capacity = 100000;
            var target = new ConcurrentFixedSizePriorityQueue<string, int>(Capacity);
            var watcher = new Stopwatch();

            watcher.Start();
            for (int i = 0; i < Capacity; i++)
            {
                target.Enqueue("a", 1);
            }
            watcher.Stop();
            Assert.AreEqual(Capacity, target.Count);
            Assert.AreEqual(Capacity, target.Capacity);
            Console.WriteLine("Enqueue {0} elements: {1}", Capacity, watcher.Elapsed);

            watcher.Restart();
            // ReSharper disable once UnusedVariable
            var enumerator = target.GetEnumerator();
            watcher.Stop();
            Console.WriteLine("Get enumerator for {0} elements: {1}", Capacity, watcher.Elapsed);

            watcher.Restart();
            for (int i = 0; i < Capacity; i++)
            {
                target.Dequeue();
            }
            watcher.Stop();
            Assert.AreEqual(0, target.Count);
            Console.WriteLine("Dequeue {0} elements: {1}", Capacity, watcher.Elapsed);

            watcher.Start();
            for (int i = 0; i < 2 * Capacity; i++)
            {
                target.Enqueue("a", 1);
            }
            watcher.Stop();
            Assert.AreEqual(Capacity, target.Count);
            Assert.AreEqual(Capacity, target.Capacity);
            Console.WriteLine("Enqueue twice the capacity of {0} elements: {1}", Capacity, watcher.Elapsed);
        }

        private void ThreadEnqueue(ConcurrentFixedSizePriorityQueue<string, DateTime> queue, int count)
        {
            var threadName = string.Format("Thread {0}", Thread.CurrentThread.ManagedThreadId);
            for (int i = 0; i < count; i++)
            {
                queue.Enqueue(threadName, new DateTime());
            }
        }

        private void ThreadDequeue(ConcurrentFixedSizePriorityQueue<string, DateTime> queue, int count)
        {
            for (int i = 0; i < count; i++)
            {
                queue.Dequeue();
            }
        }

        [Test]
        public void MultiThreadEnqueue()
        {
            const int Capacity = 100000;
            const int ThreadsCount = 100;
            const int Count = Capacity / ThreadsCount;
            var target = new ConcurrentFixedSizePriorityQueue<string, DateTime>(Capacity);

            var watcher = new Stopwatch();

            watcher.Start();
            Parallel.For(0, ThreadsCount, index => ThreadEnqueue(target, Count));
            watcher.Stop();
            Assert.AreEqual(Capacity, target.Count);
            Console.WriteLine("{0} thread each enqueue {1} elements: {2}", ThreadsCount, Count, watcher.Elapsed);

            watcher.Start();
            Parallel.For(0, ThreadsCount, index => ThreadDequeue(target, Count));
            watcher.Stop();
            Assert.AreEqual(0, target.Count);
            Console.WriteLine("{0} thread each dequeue {1} elements: {2}", ThreadsCount, Count, watcher.Elapsed);

            watcher.Start();
            Parallel.For(0, ThreadsCount, index => ThreadEnqueue(target, 2 * Count));
            watcher.Stop();
            Assert.AreEqual(Capacity, target.Count);
            Console.WriteLine("{0} thread each enqueue twice as {1} elements: {2}", ThreadsCount, Count, watcher.Elapsed);
        }
    }
}
