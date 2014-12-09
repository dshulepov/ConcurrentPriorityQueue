using System;
using System.Collections.Generic;

namespace ConcurrentPriorityQueue
{
    /// <summary>
    /// Heap-based implementation of concurrent, fixed-size priority queue. Max priority is on top of the heap.
    /// </summary>
    public class ConcurrentFixedSizePriorityQueue<TElement, TPriority> : AbstractPriorityQueue<TElement, TPriority> where TPriority : IComparable<TPriority>
    {
        private readonly object _sync = new object();

        /// <summary>
        /// Create a new instance of priority queue with given fixed capacity.
        /// </summary>
        /// <param name="capacity">Maximum queue capacity. Should be greater than 0.</param>
        /// <param name="comparer">Priority comparer. Default for type will be used unless custom is provided.</param>
        public ConcurrentFixedSizePriorityQueue(int capacity, IComparer<TPriority> comparer = null):base(capacity, comparer)
        {
        }

        private ConcurrentFixedSizePriorityQueue(Node[] nodes, int count, NodeComparer comparer):base(nodes, count, comparer)
        { }

        /// <summary>
        /// Add new item to the queue.
        /// </summary>
        public override void Enqueue(TElement item, TPriority priority)
        {
            lock (_sync)
            {
                while (_count >= Capacity) base.Dequeue();

                base.Enqueue(item, priority);
            }
        }

        /// <summary>
        /// Remove and return the item with max priority from the queue.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public override TElement Dequeue()
        {
            TElement item;
            lock (_sync)
            {
                item = base.Dequeue();
            }

            return item;
        }

        /// <summary>
        /// Remove all items from the queue. Capacity is not changed.
        /// </summary>
        public override void Clear()
        {
            lock (_sync)
            {
                base.Clear();
            }
        }

        /// <summary>
        /// Returns true if there is at least one item, which is equal to given.
        /// TD.Equals is used to compare equality.
        /// </summary>
        public override bool Contains(TElement item)
        {
            lock (_sync)
            {
                return base.Contains(item);
            }
        }

        /// <summary>
        /// Returns the first element in the queue (element with max priority) without removing it from the queue.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public override TElement Peek()
        {
            lock (_sync)
            {
                return base.Peek();
            }
        }

        /// <summary>
        /// Update priority of the first occurrence of the given item
        /// </summary>
        /// <param name="item">Item, which priority should be updated.</param>
        /// <param name="priority">New priority</param>
        /// <exception cref="ArgumentException"></exception>
        public override void UpdatePriority(TElement item, TPriority priority)
        {
            lock (_sync)
            {
                base.UpdatePriority(item, priority);
            }
        }

        public override IEnumerator<TElement> GetEnumerator()
        {
            Node[] nodesCopy;
            lock (_sync)
            {
                nodesCopy = CopyNodes();
            }
            // queue copy is created to be able to extract the items in the priority order
            // using the already existing dequeue method
            // (because they are not exactly in priority order in the underlying array)
            var queueCopy = new ConcurrentFixedSizePriorityQueue<TElement, TPriority>(nodesCopy, nodesCopy.Length - 1, _comparer);

            return new PriorityQueueEnumerator(queueCopy);
        }
    }
}
