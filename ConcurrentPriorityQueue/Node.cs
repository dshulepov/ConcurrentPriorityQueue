using System;

namespace ConcurrentPriorityQueue
{
    internal class Node<TKey, TValue> where TKey : IComparable<TKey>
    {
        public TKey Key;
        public TValue Value;

        public Node(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }
}
