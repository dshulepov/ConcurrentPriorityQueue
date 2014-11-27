using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentPriorityQueue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConcurrentPriorityQueueTests.PerformanceTests
{
    [TestClass]
    public class ConcurrentFixedSizePriorityQueueTests
    {

        [TestMethod]
        public void SingleThreadTiming()
        {
            const int Capacity = 1000000;
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

        [TestMethod]
        public void MultiThreadEnqueue()
        {
            const int Capacity = 1000000;
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
