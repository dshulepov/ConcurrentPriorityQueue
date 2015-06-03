using System;
using System.Collections.Generic;
using ConcurrentPriorityQueue;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ConcurrentPriorityQueueTests.FunctionalTests
{
    [TestClass]
    public class SkipListTests
    {
        [TestMethod]
        public void EmptySkipListTests()
        {
            var target = new SkipList<int, string>();

            Assert.AreEqual(0, target.Count);
            var throws = false;
            try
            {
                var a = target[1];
            }
            catch (KeyNotFoundException)
            {
                throws = true;
            }
            Assert.IsTrue(throws);

            var enumerator = target.GetEnumerator();
            Assert.IsFalse(enumerator.MoveNext());
        }

        [TestMethod]
        public void SimpleAdditionTests()
        {
            var target = new SkipList<int, string>();

            target.Add(2, "two");

            Assert.AreEqual(1, target.Count);
            Assert.AreEqual("two", target[2]);

            target.Add(1, "one");
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual("one", target[1]);
            Assert.AreEqual("two", target[2]);

            target.Add(3, "three");
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual("one", target[1]);
            Assert.AreEqual("two", target[2]);
            Assert.AreEqual("three", target[3]);
        }

        [TestMethod]
        public void RandomizedAdditionTests()
        {
            var target = new SkipList<int, int>();
            var random = new Random();
            const int count = 200;
            var store = new int[count];

            int i;
            for (i = 0; i < count; i++)
            {
                store[i] = random.Next(count + 1);
                target.Add(store[i], store[i]);

                //DebugOutputStructure(target);

                Assert.AreEqual(i + 1, target.Count);
                for (int j = 0; j <= i; j++)
                {
                    Assert.AreEqual(store[j], target[store[j]]);
                }
            }

            Array.Sort(store);
            i = 0;
            foreach (var pair in target)
            {
                Assert.AreEqual(store[i], pair.Key);
                i++;
            }
        }

        [TestMethod]
        public void SimpleIndexerTests()
        {
            var target = new SkipList<int, int>();

            target[3] = 1;
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(1, target[3]);

            target[3] = 2;
            Assert.AreEqual(1, target.Count);
            Assert.AreEqual(2, target[3]);

            target[2] = 2;
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(2, target[3]);
            Assert.AreEqual(2, target[2]);

            target[3] = 3;
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual(3, target[3]);
            Assert.AreEqual(2, target[2]);

            target[1] = 1;
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual(1, target[1]);
            Assert.AreEqual(2, target[2]);
            Assert.AreEqual(3, target[3]);
        }

        [TestMethod]
        public void RandomizedIndexerTests()
        {
            var target = new SkipList<int, int>();
            var random = new Random();
            const int count = 200;
            var store = new Dictionary<int, int>();

            int i;
            for (i = 0; i < count; i++)
            {
                int r = random.Next(count + 1);
                if (store.ContainsKey(r))
                {
                    store[r]++;
                }
                else
                {
                    store[r] = 1;
                }

                if (target.ContainsKey(r))
                {
                    target[r]++;
                }
                else
                {
                    target[r] = 1;
                }

                //DebugOutputStructure(target);

                Assert.AreEqual(store.Count, target.Count);
                foreach (var pair in store)
                {
                    Assert.AreEqual(pair.Value, target[pair.Key]);
                }
            }
        }

        private void DebugOutputStructure<TKey, TValue>(SkipList<TKey, TValue> target) where TKey: IComparable<TKey>
        {
            var nodes = new SkipList<TKey, TValue>.SkipListNode[target.Count];
            var node = target._head.GetNext(0);
            for (int j = 0; j < target.Count; j++)
            {
                nodes[j] = node;
                node = node.GetNext(0);
            }

            Console.WriteLine("=======================");
            for (int i = 0; i < target._height; i++)
            {
                Console.Write("{0:00}", i);
                if (target._head.Height > i) Console.Write("H-");
                for (int j = 0; j < target.Count; j++)
                {
                    node = nodes[j];

                    if (node.Height > i)
                    {
                        Console.Write("{0:00}-", node.Key);
                    }
                    else
                    {
                        Console.Write("---");
                    }

                }
                if (target._tail.Height > i) Console.WriteLine("T");
            }
        }
    }
}
