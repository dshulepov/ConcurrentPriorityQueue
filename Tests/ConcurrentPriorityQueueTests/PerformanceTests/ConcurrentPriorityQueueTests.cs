using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using ConcurrentPriorityQueue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConcurrentPriorityQueueTests.PerformanceTests
{
    [TestClass]
    public class ConcurrentPriorityQueueTests
    {
        [TestMethod]
        public void SingleThreadTiming()
        {
            const int Count = 1000000;
            var target = new ConcurrentPriorityQueue<string, int>();
            var watcher = new Stopwatch();

            watcher.Start();
            for (int i = 0; i < Count; i++)
            {
                target.Enqueue("a", 1);
            }
            watcher.Stop();
            Assert.AreEqual(Count, target.Count);
            // TODO check capacity
            Console.WriteLine("Enqueue {0} elements: {1}", Count, watcher.Elapsed);

            watcher.Restart();
            // ReSharper disable once UnusedVariable
            var enumerator = target.GetEnumerator();
            watcher.Stop();
            Console.WriteLine("Get enumerator for {0} elements: {1}", Count, watcher.Elapsed);

            watcher.Restart();
            for (int i = 0; i < Count; i++)
            {
                target.Dequeue();
            }
            watcher.Stop();
            Assert.AreEqual(0, target.Count);
            // TODO check capcity
            Console.WriteLine("Dequeue {0} elements: {1}", Count, watcher.Elapsed);

            watcher.Start();
            for (int i = 0; i < 2 * Count; i++)
            {
                target.Enqueue("a", 1);
            }
            watcher.Stop();
            Assert.AreEqual(2 * Count, target.Count);
            // TODO check capcity
            Console.WriteLine("Enqueue twice the capacity of {0} elements: {1}", Count, watcher.Elapsed);
        }

        private void ThreadEnqueue(ConcurrentPriorityQueue<string, DateTime> queue, int count)
        {
            var threadName = string.Format("Thread {0}", Thread.CurrentThread.ManagedThreadId);
            for (int i = 0; i < count; i++)
            {
                queue.Enqueue(threadName, new DateTime());
            }
        }

        private void ThreadDequeue(ConcurrentPriorityQueue<string, DateTime> queue, int count)
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
            var target = new ConcurrentPriorityQueue<string, DateTime>(Capacity);

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
            Assert.AreEqual(2 * Capacity, target.Count);
            Console.WriteLine("{0} thread each enqueue twice as {1} elements: {2}", ThreadsCount, Count, watcher.Elapsed);
        }
    }
}
