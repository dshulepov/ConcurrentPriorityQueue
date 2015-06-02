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
                target.GetValue(1);
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
            Assert.AreEqual("two", target.GetValue(2));

            target.Add(1, "one");
            Assert.AreEqual(2, target.Count);
            Assert.AreEqual("one", target.GetValue(1));
            Assert.AreEqual("two", target.GetValue(2));

            target.Add(3, "three");
            Assert.AreEqual(3, target.Count);
            Assert.AreEqual("one", target.GetValue(1));
            Assert.AreEqual("two", target.GetValue(2));
            Assert.AreEqual("three", target.GetValue(3));
        }

        [TestMethod]
        public void RandomAdditionTests()
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
                    Assert.AreEqual(store[j], target.GetValue(store[j]));
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
            for (int i = 0; i < target._levels; i++)
            {
                Console.Write("{0:00}", i);
                if (target._head.Levels > i) Console.Write("H-");
                for (int j = 0; j < target.Count; j++)
                {
                    node = nodes[j];

                    if (node.Levels > i)
                    {
                        Console.Write("{0:00}-", node.Key);
                    }
                    else
                    {
                        Console.Write("---");
                    }

                }
                if (target._tail.Levels > i) Console.WriteLine("T");
            }
        }
    }
}
