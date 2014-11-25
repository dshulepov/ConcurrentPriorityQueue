using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentPriorityQueue
{
    /// <summary>
    /// Heap-based implementation of concurrent, fixed-size priority queue.
    /// </summary>
    public class ConcurrentFixedSizePriorityQueue<TD, TK> : AbstractPriorityQueue<TD, TK>, IEnumerable<TD> where TK : IComparable<TK>
    {
        private readonly object _sync = new object();

        /// <summary>
        /// Create a new instance of priority queue with given fixed capacity.
        /// </summary>
        /// <param name="capacity">Maximum queue capacity. Should be greater than 0.</param>
        /// <param name="comparer">Priority comparer. Default for type will be used unless custom is provided.</param>
        public ConcurrentFixedSizePriorityQueue(int capacity, IComparer<TK> comparer = null):base(capacity, comparer)
        {
        }

        private ConcurrentFixedSizePriorityQueue(Node[] nodes, int count, NodeComparer comparer):base(nodes, count, comparer)
        { }

        public override void Enqueue(TD item, TK priority)
        {
            lock (_sync)
            {
                while (_count >= Capacity) base.Dequeue();

                base.Enqueue(item, priority);
            }
        }

        public override TD Dequeue()
        {
            TD item;
            lock (_sync)
            {
                item = base.Dequeue();
            }

            return item;
        }

        public override void Clear()
        {
            lock (_sync)
            {
                base.Clear();
            }
        }

        public IEnumerator<TD> GetEnumerator()
        {
            Node[] nodesCopy;
            lock (_sync)
            {
                nodesCopy = CopyNodes();
            }
            // queue copy is created to be able to extract the items in the priority order
            // using the already existing dequeue method
            // (because they are not exactly in priority order in the underlying array)
            var queueCopy = new ConcurrentFixedSizePriorityQueue<TD, TK>(nodesCopy, nodesCopy.Length - 1, _comparer);

            return new PriorityQueueEnumerator(queueCopy);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
