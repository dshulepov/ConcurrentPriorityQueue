using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConcurrentPriorityQueue
{
    /// <summary>
    /// Heap-based implementation of concurrent priority queue.
    /// </summary>
    public class ConcurrentPriorityQueue<TD, TK> : AbstractPriorityQueue<TD, TK>, IEnumerable<TD> where TK : IComparable<TK>
    {
        private readonly object _sync = new object();
        private const int _defaultCapacity = 10;
        private const int _resizeFactor = 2;
        private const int _shrinkRatio = 4;

        private int _shrinkBound;

        /// <summary>
        /// Create a new instance of priority queue with given initial capacity.
        /// </summary>
        /// <param name="capacity">Initial queue capacity. Should be greater than 0.</param>
        /// <param name="comparer">Priority comparer. Default for type will be used unless custom is provided.</param>
        public ConcurrentPriorityQueue(int capacity, IComparer<TK> comparer = null):base(capacity, comparer)
        {
            _shrinkBound = Capacity / _shrinkRatio;
        }

        public ConcurrentPriorityQueue(IComparer<TK> comparer = null): this(_defaultCapacity, comparer)
        {
        }

        private ConcurrentPriorityQueue(Node[] nodes, int count, NodeComparer comparer):base(nodes, count, comparer)
        { }

        public override void Enqueue(TD item, TK priority)
        {
            lock (_sync)
            {
                if (_count == Capacity) GrowCapacity();

                base.Enqueue(item, priority);
            }
        }

        public override TD Dequeue()
        {
            TD item;
            lock (_sync)
            {
                item = base.Dequeue();

                if (_count <= _shrinkBound && _count > _defaultCapacity) ShrinkCapacity();
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
            var queueCopy = new ConcurrentPriorityQueue<TD, TK>(nodesCopy, nodesCopy.Length - 1, _comparer);

            return new PriorityQueueEnumerator(queueCopy);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private void GrowCapacity()
        {
            int newCapacity = Capacity * _resizeFactor;
            Array.Resize(ref _nodes, newCapacity + 1);  // first element is at position 1
            _shrinkBound = newCapacity / _shrinkRatio;
        }

        private void ShrinkCapacity()
        {
            int newCapacity = Capacity / _resizeFactor;
            Array.Resize(ref _nodes, newCapacity + 1);  // first element is at position 1
            _shrinkBound = newCapacity / _shrinkRatio;
        }
    }
}
