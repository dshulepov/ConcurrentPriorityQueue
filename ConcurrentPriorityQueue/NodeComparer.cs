using System;
using System.Collections.Generic;

namespace ConcurrentPriorityQueue
{
    internal class NodeComparer<TKey, TValue> where TKey : IComparable<TKey>
    {
        private readonly IComparer<TKey> _comparer;

        public NodeComparer(IComparer<TKey> comparer)
        {
            _comparer = comparer;
        }

        public int Compare(Node<TKey, TValue> x, Node<TKey, TValue> y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            return _comparer.Compare(x.Key, y.Key);
        }

        public int Compare(TKey x, TKey y)
        {
            return _comparer.Compare(x, y);
        }
    }
}
