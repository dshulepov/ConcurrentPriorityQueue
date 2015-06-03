using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConcurrentPriorityQueue
{
    public class SkipList<TKey, TValue> : IDictionary<TKey, TValue> where TKey:IComparable<TKey>
    {
        internal readonly SkipListHeadNode _head;
        internal readonly SkipListTailNode _tail;
        internal int _height;
        private int _count;
        private readonly NodeComparer<TKey, TValue> _nodeComparer;
        private readonly Random _random;
        private readonly int _maxHeight;

        public SkipList(IComparer<TKey> comparer = null, int maxHeight = 16)
        {
            _nodeComparer = new NodeComparer<TKey, TValue>(comparer ?? Comparer<TKey>.Default);
            _random = new Random();
            _count = 0;
            _height = 1;
            _maxHeight = maxHeight;

            _head = new SkipListHeadNode(maxHeight);
            _tail = new SkipListTailNode(maxHeight);
            for (int i = 0; i < maxHeight; i++)
            {
                _head.SetNext(i, _tail);
                _tail.SetPrev(i, _head);
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            Add(item.Key, item.Value);
        }

        public void Clear()
        {
            for (int i = 0; i < _maxHeight; i++)
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
            
            AddNew(key, value, prev);
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
                    AddNew(key, value, prev);
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
            SkipListNode node = _head;

            while (level >= 0)
            {
                var next = node.GetNext(level);
                int cmp;
                while ((cmp = CompareNode(next, key)) < 0)
                {
                    node = next;
                    next = next.GetNext(level);
                }
                if (cmp == 0) return next; 

                level--;
            }
            return node;
        }

        private void AddNew(TKey key, TValue value, SkipListNode prev)
        {
            var next = prev.GetNext(0);

            var newLevels = GetNewNodeHeight();

            var newNode = new SkipListNode(key, value, newLevels);
            InsertNode(newNode, newLevels, prev, next);
            _count++;
        }

        private int GetNewNodeHeight()
        {
            int height = 1;
            if (_height == _maxHeight)
            {
                height += _random.Next(_height);
            }
            else
            {
                height += _random.Next(_height + 1);
            }

            if (height > _height) _height = height;
            return height;
        }

        private void InsertNode(SkipListNode node, int height, SkipListNode prev, SkipListNode next)
        {
            for (int i = 0; i < height; i++)
            {
                while (prev.Height <= i) prev = prev.GetPrev(i - 1);
                while (next.Height <= i) next = next.GetNext(i - 1);
                node.SetPrev(i, prev);
                node.SetNext(i, next);

                prev.SetNext(i, node);
                next.SetPrev(i, node);
            }
        }

        private void DeleteNode(SkipListNode node)
        {
            var prev = node.GetPrev(0);
            var next = node.GetNext(0);
            for (int i = 0; i < node.Height; i++)
            {
                while (prev.Height <= i) prev = prev.GetPrev(i - 1);
                while (next.Height <= i) next = next.GetNext(i - 1);

                prev.SetNext(i, next);
                next.SetPrev(i, prev);
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
            protected SkipListNode[] _next;
            protected SkipListNode[] _prev;

            protected internal SkipListNode(TKey key, TValue value, int height) : base(key, value)
            {
                _next = new SkipListNode[height];
                _prev = new SkipListNode[height];
            }

            internal virtual int Height { get { return _next.Length; } }

            public virtual SkipListNode GetNext(int level)
            {
                return _next[level];
            }

            public virtual void SetNext(int level, SkipListNode node)
            {
                _next[level] = node;
            }

            public virtual void SetPrev(int level, SkipListNode node)
            {
                _prev[level] = node;
            }

            public virtual SkipListNode GetPrev(int level)
            {
                return _prev[level];
            }

            public override string ToString()
            {
                return string.Format("Node [{0},{1}] ({2})", Key, Value, Height);
            }
        }

        [DebuggerDisplay("Head ({Height})")]
        internal class SkipListHeadNode : SkipListNode
        {
            protected internal SkipListHeadNode(int height) : base(default(TKey), default(TValue), height)
            {
            }

            public override SkipListNode GetPrev(int level)
            {
                throw new InvalidOperationException("There is no element preceding head.");
            }

            public override void SetPrev(int level, SkipListNode node)
            {
                throw new InvalidOperationException("Not allowed to set element preceding head.");
            }

            public override string ToString()
            {
                return string.Format("Head ({0})", Height);
            }
        }

        [DebuggerDisplay("Tail ({Height})")]
        internal class SkipListTailNode : SkipListNode
        {
            protected internal SkipListTailNode(int height) : base(default(TKey), default(TValue), height)
            {
            }

            internal override int Height { get { return _prev.Length; } }

            public override SkipListNode GetNext(int level)
            {
                throw new InvalidOperationException("There is no element following tail.");
            }

            public override void SetNext(int level, SkipListNode node)
            {
                throw new InvalidOperationException("Not allowed to set element following tail.");
            }

            public override string ToString()
            {
                return string.Format("Tail ({0})", Height);
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
