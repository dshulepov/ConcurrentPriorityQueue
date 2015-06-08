using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConcurrentPriorityQueue
{
    public class SkipList<TKey, TValue> : IDictionary<TKey, TValue> where TKey:IComparable<TKey>
    {
        internal readonly SkipListNode _head;
        internal readonly SkipListNode _tail;
        internal int _height;
        private int _count;
        private readonly NodeComparer<TKey, TValue> _nodeComparer;
        private readonly Random _random;
        private const int MAX_HEIGHT = 32;
        internal const int HEIGHT_STEP = 4;
        private SkipListNode _lastFoundNode;

        public SkipList(IComparer<TKey> comparer = null)
        {
            _nodeComparer = new NodeComparer<TKey, TValue>(comparer ?? Comparer<TKey>.Default);
            _random = new Random();
            _count = 0;
            _height = 1;

            _head = new SkipListNode(default(TKey), default(TValue), HEIGHT_STEP);
            _tail = new SkipListNode(default(TKey), default(TValue), HEIGHT_STEP);
            for (int i = 0; i < HEIGHT_STEP; i++)
            {
                _head.SetNext(i, _tail);
                _tail.SetPrev(i, _head);
            }
            _lastFoundNode = _head;
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            for (int i = 0; i < MAX_HEIGHT; i++)
            {
                _head.SetNext(i, _tail);
                _tail.SetPrev(i, _head);
            }
            _count = 0;
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            var node = FindNode(item.Key);

            while (CompareNode(node, item.Key) == 0)
            {
                if (EqualityComparer<TValue>.Default.Equals(node.Value, item.Value)) return true;
                node = node.GetNext(0);
            }
            return false;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            var node = FindNode(item.Key);
            var next = node.GetNext(0);

            while (CompareNode(node, item.Key) == 0)
            {
                if (EqualityComparer<TValue>.Default.Equals(node.Value, item.Value))
                {
                    DeleteNode(node);
                    return true;
                }
                node = next;
                next = next.GetNext(0);
            }
            return false;
        }

        public int Count { get { return _count; } }
        public bool IsReadOnly { get; private set; }

        public void Add(TKey key, TValue value)
        {
            var prev = FindNode(key);
            
            AddNewNode(key, value, prev);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            throw new NotImplementedException();
        }

        public TValue this[TKey key]
        {
            get
            {
                var node = FindNode(key);
                if (CompareNode(node, key) == 0) return node.Value;
                throw new KeyNotFoundException();
            }
            set
            {
                var prev = FindNode(key);
                if (CompareNode(prev, key) == 0)
                {
                    prev.Value = value;
                }
                else
                {
                    AddNewNode(key, value, prev);
                }
            }
        }

        public ICollection<TKey> Keys { get; private set; }
        public ICollection<TValue> Values { get; private set; }

        public bool ContainsKey(TKey key)
        {
            var node = FindNode(key);
            return CompareNode(node, key) == 0;
        }

        public bool Remove(TKey key)
        {
            var node = FindNode(key);
            if (CompareNode(node, key) != 0) return false;

            DeleteNode(node);

            return true;
        }

        private SkipListNode FindNode(TKey key)
        {
            var level = _height - 1;
            var node = _head;
            int cmp;
            if (_lastFoundNode != _head)
            {
                if ((cmp = CompareNode(_lastFoundNode, key)) == 0) return _lastFoundNode;
                if (cmp < 0)
                {
                    node = _lastFoundNode;
                    level = _lastFoundNode.Height - 1;
                }
            }

            while (level >= 0)
            {
                var next = node.GetNext(level);
                while ((cmp = CompareNode(next, key)) < 0)
                {
                    node = next;
                    next = next.GetNext(level);
                }
                if (cmp == 0)
                {
                    _lastFoundNode = next;
                    return next;
                } 

                level--;
            }
            _lastFoundNode = node;
            return node;
        }

        private void AddNewNode(TKey key, TValue value, SkipListNode prev)
        {
            var next = prev.GetNext(0);
            var newNodeHeight = GetNewNodeHeight();

            var newNode = new SkipListNode(key, value, newNodeHeight);
            InsertNode(newNode, newNodeHeight, prev, next);
            _count++;
        }

        private int GetNewNodeHeight()
        {
            var maxNodeHeight = _height;
            if (maxNodeHeight < 32 && (1 << maxNodeHeight) < _count)
            {
                maxNodeHeight++;
            }
            var nodeHeight = 1 + _random.Next(maxNodeHeight);
            if (nodeHeight > _height)
            {
                _height = nodeHeight;
                if (_head.Height < _height)
                {
                    maxNodeHeight = _head.Height;
                    _head.Grow(maxNodeHeight + HEIGHT_STEP);
                    _tail.Grow(maxNodeHeight + HEIGHT_STEP);
                    while (maxNodeHeight < _head.Height)
                    {
                        _head.SetNext(maxNodeHeight, _tail);
                        _tail.SetPrev(maxNodeHeight, _head);
                        maxNodeHeight++;
                    }
                }
            }
            return nodeHeight;
        }

        private void InsertNode(SkipListNode newNode, int height, SkipListNode prev, SkipListNode next)
        {
            for (int i = 0; i < height; i++)
            {
                while (prev.Height <= i) prev = prev.GetPrev(i - 1);
                while (next.Height <= i) next = next.GetNext(i - 1);
                newNode.SetPrev(i, prev);
                newNode.SetNext(i, next);

                prev.SetNext(i, newNode);
                next.SetPrev(i, newNode);
            }
        }

        private void DeleteNode(SkipListNode node)
        {
            for (int i = 0; i < node.Height; i++)
            {
                var prev = node.GetPrev(i);
                var next = node.GetNext(i);

                while (prev.Height <= i) prev = prev.GetPrev(i - 1);
                while (next.Height <= i) next = next.GetNext(i - 1);

                prev.SetNext(i, next);
                next.SetPrev(i, prev);
            }

            _lastFoundNode = _head;
            _count--;

            if (_height > 1 && (1 << _height) > _count)
            {
                _height--;
            }
        }

        private int CompareNode(SkipListNode node, TKey key)
        {
            if (node == _head) return -1;
            if (node == _tail) return 1;

            return _nodeComparer.Compare(node.Key, key);
        }

        [DebuggerDisplay("Node [{Key},{Value}] ({Height})")]
        internal class SkipListNode : Node<TKey, TValue>
        {
            private SkipListNode[] _next;
            private SkipListNode[] _prev;

            protected internal SkipListNode(TKey key, TValue value, int height) : base(key, value)
            {
                _next = new SkipListNode[height];
                _prev = new SkipListNode[height];
            }

            internal int Height { get { return _next.Length; } }

            public SkipListNode GetNext(int level)
            {
                return _next[level];
            }

            public void SetNext(int level, SkipListNode node)
            {
                _next[level] = node;
            }

            public void SetPrev(int level, SkipListNode node)
            {
                _prev[level] = node;
            }

            public SkipListNode GetPrev(int level)
            {
                return _prev[level];
            }

            internal void Grow(int height)
            {
                var newNext = new SkipListNode[height];
                var newPrev = new SkipListNode[height];
                Array.Copy(_next, newNext, _next.Length);
                Array.Copy(_prev, newPrev, _prev.Length);
                _next = newNext;
                _prev = newPrev;
            }
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var items = new LinkedList<KeyValuePair<TKey, TValue>>();
            SkipListNode node = _head.GetNext(0);
            while (node != _tail)
            {
                items.AddLast(new KeyValuePair<TKey, TValue>(node.Key, node.Value));
                node = node.GetNext(0);
            }
            return items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
