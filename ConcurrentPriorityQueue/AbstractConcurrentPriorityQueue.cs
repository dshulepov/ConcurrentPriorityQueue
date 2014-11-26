using System;
using System.Collections;
using System.Collections.Generic;

namespace ConcurrentPriorityQueue
{
    /// <summary>
    /// Heap-based implementation of priority queue.
    /// </summary>
    public abstract class AbstractPriorityQueue<TD, TK> where TK : IComparable<TK>
    {
        internal sealed class Node
        {
            public readonly TK Key;
            public readonly TD Data;

            public Node(TD data, TK key)
            {
                Key = key;
                Data = data;
            }
        }

        internal Node[] _nodes;
        internal int _count;
        internal readonly NodeComparer _comparer;

        internal AbstractPriorityQueue(int capacity, IComparer<TK> comparer = null)
        {
            if (capacity <= 0) throw new ArgumentOutOfRangeException("capacity", "Expected capacity greater than zero.");

            _nodes = new Node[capacity + 1];        // first element at 1
            _count = 0;
            _comparer = new NodeComparer(comparer ?? Comparer<TK>.Default);
        }

        internal AbstractPriorityQueue(Node[] nodes, int count, NodeComparer comparer)
        {
            _nodes = nodes;
            _count = count;
            _comparer = comparer;
        }

        public int Capacity { get { return _nodes.Length - 1; } }

        public int Count { get { return _count; } }

        public virtual void Enqueue(TD item, TK priority)
        {
            int index = _count + 1;
            _nodes[index] = new Node(item, priority);

            _count = index;             // update count after the element is really added but before Sift

            Sift(index);
        }

        public virtual TD Dequeue()
        {
            if (_count == 0) throw new InvalidOperationException("Unable to dequeue from empty queue.");

            TD item = _nodes[1].Data;   // first element at 1
            Swap(1, _count);            // last element at _count
            _nodes[_count] = null;      // release hold on the object

            _count--;                   // update count after the element is really gone but before Sink

            Sink(1);

            return item;
        }

        public virtual void Clear()
        {
            for (int i = 1; i <= _count; i++)
            {
                _nodes[i] = null;
            }
            _count = 0;
        }

        internal Node[] CopyNodes()
        {
            Node[] nodesCopy = new Node[_count + 1];
            Array.Copy(_nodes, 0, nodesCopy, 0, _count + 1);
            return nodesCopy;
        }

        private bool GreaterOrEqual(Node i, Node j)
        {
            return _comparer.Compare(i, j) >= 0;
        }

        private void Sink(int i)
        {
            while (true)
            {
                int leftChildIndex = 2 * i;
                int rightChildIndex = 2 * i + 1;
                if (leftChildIndex > _count) return; // reached last item

                var item = _nodes[i];
                var left = _nodes[leftChildIndex];
                var right = rightChildIndex > _count ? null : _nodes[rightChildIndex];

                // if item is greater than children - exit
                if (GreaterOrEqual(item, left) && (right == null || GreaterOrEqual(item, right))) return;

                // else exchange with greater of children
                int greaterChild = right == null || GreaterOrEqual(left, right) ? leftChildIndex : rightChildIndex;
                Swap(i, greaterChild);

                // continue at new position
                i = greaterChild;
            }
        }

        private void Sift(int i)
        {
            while (true)
            {
                if (i <= 1) return;         // reached root
                int parent = i / 2;         // get parent

                // if root is greater or equal - exit
                if (GreaterOrEqual(_nodes[parent], _nodes[i])) return;

                Swap(parent, i);
                i = parent;
            }
        }

        private void Swap(int i, int j)
        {
            var tmp = _nodes[i];
            _nodes[i] = _nodes[j];
            _nodes[j] = tmp;
        }

        internal sealed class NodeComparer : IComparer<Node>
        {
            private readonly IComparer<TK> _comparer;

            public NodeComparer(IComparer<TK> comparer)
            {
                _comparer = comparer;
            }

            public int Compare(Node x, Node y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                return _comparer.Compare(x.Key, y.Key);
            }
        }

        internal sealed class PriorityQueueEnumerator : IEnumerator<TD>
        {
            private readonly TD[] _items;
            private int _currentIndex;

            public PriorityQueueEnumerator(AbstractPriorityQueue<TD, TK> queueCopy)
            {
                _items = new TD[queueCopy.Count];

                for (int i = 0; i < _items.Length; i++)
                {
                    _items[i] = queueCopy.Dequeue();
                }

                Reset();
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                _currentIndex++;
                return _currentIndex < _items.Length;
            }

            public void Reset()
            {
                _currentIndex = -1;
            }

            public TD Current { get { return _items[_currentIndex]; } }

            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }
}
