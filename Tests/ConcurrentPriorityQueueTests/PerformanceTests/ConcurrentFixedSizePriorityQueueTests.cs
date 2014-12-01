using System;
using System.Collections.Generic;
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
            const int capacity = 1000000;
            var target = new ConcurrentFixedSizePriorityQueue<string, int>(capacity);
            var watcher = new Stopwatch();

            watcher.Start();
            for (int i = 0; i < capacity; i++)
            {
                target.Enqueue("a", 1);
            }
            watcher.Stop();
            Assert.AreEqual(capacity, target.Count);
            Assert.AreEqual(capacity, target.Capacity);
            Console.WriteLine("Enqueue {0} elements: {1}", capacity, watcher.Elapsed);

            watcher.Restart();
            // ReSharper disable once UnusedVariable
            var enumerator = target.GetEnumerator();
            watcher.Stop();
            Console.WriteLine("Get enumerator for {0} elements: {1}", capacity, watcher.Elapsed);

            watcher.Restart();
            for (int i = 0; i < capacity; i++)
            {
                target.Dequeue();
            }
            watcher.Stop();
            Assert.AreEqual(0, target.Count);
            Console.WriteLine("Dequeue {0} elements: {1}", capacity, watcher.Elapsed);

            watcher.Start();
            for (int i = 0; i < 2 * capacity; i++)
            {
                target.Enqueue("a", 1);
            }
            watcher.Stop();
            Assert.AreEqual(capacity, target.Count);
            Assert.AreEqual(capacity, target.Capacity);
            Console.WriteLine("Enqueue twice the capacity of {0} elements: {1}", capacity, watcher.Elapsed);
        }

        [TestMethod]
        public void MultiThreadEnqueue()
        {
            const int capacity = 1000000;
            const int threadsCount = 100;
            const int count = capacity / threadsCount;
            var target = new ConcurrentFixedSizePriorityQueue<string, DateTime>(capacity);

            var execStats = new ExecWithStats[threadsCount];

            var watcher = new Stopwatch();

            // several threads enqueue elements
            watcher.Start();
            Parallel.For(0, threadsCount, index =>
            {
                execStats[index] = new ExecWithStats(string.Format("Enqueue {0}", count), count, () => target.Enqueue("a", new DateTime()));
                execStats[index].Exec();
            });

            watcher.Stop();
            Assert.AreEqual(capacity, target.Count);
            Console.WriteLine("{0} threads each enqueue {1} elements. total time: {2}\n", threadsCount, count, watcher.Elapsed);
            ExecWithStats.OutputStatsSummary(execStats);

            // several threads dequeue elements
            watcher.Start();
            Parallel.For(0, threadsCount, index =>
            {
                execStats[index] = new ExecWithStats(string.Format("Dequeue {0}", count), count, () => target.Dequeue());
                execStats[index].Exec();
            });
            watcher.Stop();
            Assert.AreEqual(0, target.Count);
            Console.WriteLine("\n{0} threads each dequeue {1} elements. total time: {2}\n", threadsCount, count, watcher.Elapsed);
            ExecWithStats.OutputStatsSummary(execStats);

            // several threads enqueue double amount of elements
            // so on the second half each enqueue will have to do a dequeue because the queue will be full
            watcher.Start();
            Parallel.For(0, threadsCount, index =>
            {
                execStats[index] = new ExecWithStats(string.Format("Enqueue {0}", 2 * count), 2 * count, () => target.Enqueue("a", new DateTime()));
                execStats[index].Exec();
            });
            watcher.Stop();
            Assert.AreEqual(capacity, target.Count);
            Console.WriteLine("\n{0} threads each enqueue {1} elements. total time: {2}\n", threadsCount, 2 * count, watcher.Elapsed);
            ExecWithStats.OutputStatsSummary(execStats);
        }

        [TestMethod]
        public void RaceWithStats()
        {
            const int capacity = 1000000;
            const int threadsCount = 100;
            const int count = capacity / threadsCount;
            var target = new ConcurrentFixedSizePriorityQueue<string, DateTime>(capacity);
            var execStats = new List<ExecWithStats>();
            var threadWait = new CountdownEvent(threadsCount);

            // odd threads will enqueue elements, while even threads will dequeue
            // obviously there will be a race condition and especially in the beginning dequeue will throw, because queue will often be empty
            // the total number of exceptions on dequeue threads will correspond the the number of items left in the queue

            for (var i = 0; i < threadsCount; i++)
            {
                ExecWithStats exec;
                if (i % 2 != 0)
                {
                    exec = new ExecWithStats(string.Format("Enqueue {0} elements", count), count, () => target.Enqueue("a", new DateTime()), threadWait);
                }
                else
                {
                    exec = new ExecWithStats(string.Format("Dequeue {0} elements", count), count, () => target.Dequeue(), threadWait);
                }

                execStats.Add(exec);

                var thread = new Thread(() => exec.Exec());
                thread.Start();
            }

            // Wait for all threads in pool to calculate.
            threadWait.Wait();

            // Output stats summary
            ExecWithStats.OutputStatsSummary(execStats);

            // Output queue state
            Console.WriteLine("Queue count:{0}, capacity:{1}", target.Count, target.Capacity);

            // Un-comment for a detailed list of stats
            //Console.WriteLine("---------------------");
            //foreach (var execStat in execStats)
            //{
            //    var stats = execStat.GetStats();
            //    Console.WriteLine("Name:{0}, Min: {1}, Median: {2}, Max {3}, Exceptions: {4}", stats.Name, stats.Min, stats.Med, stats.Max, stats.ExceptionsCount);
            //}
        }
    }
}
