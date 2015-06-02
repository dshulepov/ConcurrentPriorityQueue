using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConcurrentPriorityQueue
{
    public class SkipList<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> where TKey:IComparable<TKey>
    {
        internal SkipListHeadNode _head;
        internal SkipListTailNode _tail;
        internal int _levels;
        private int _count;
        private readonly NodeComparer<TKey, TValue> _nodeComparer;
        private readonly Random _random;

        public SkipList(IComparer<TKey> comparer = null)
        {
            _nodeComparer = new NodeComparer<TKey, TValue>(comparer ?? Comparer<TKey>.Default);
            _random = new Random();
            _count = 0;
            _levels = 1;

            _head = new SkipListHeadNode();
            _tail = new SkipListTailNode();
            _head.SetNext(0, _tail);
            _tail.SetPrev(0, _head);
        }

        public int Count { get { return _count; } }

        public TValue GetValue(TKey key)
        {
            var node = FindNode(key);
            if (CompareNode(node, key) == 0) return node.Value;
            throw new KeyNotFoundException();
        }

        public void Add(TKey key, TValue value)
        {
            var prev = FindNode(key);
            var next = prev.GetNext(0);

            var newLevels = 1 + _random.Next(_levels + 1);
            if (newLevels > _levels)
            {
                _head.LevelsUp(newLevels);
                _tail.LevelsUp(newLevels);
                _head.SetNext(_levels, _tail);
                _tail.SetPrev(_levels, _head);
                _levels = newLevels;
            }

            var newNode = new SkipListNode(key, value, newLevels);
            InsertNode(newNode, newLevels, prev, next);
            _count++;
        }

        private SkipListNode FindNode(TKey key)
        {
            var level = _levels - 1;
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
                if (cmp == 0)
                {
                    return next;
                }
                level--;
            }
            return node;
        }

        private void InsertNode(SkipListNode node, int levels, SkipListNode prev, SkipListNode next)
        {
            for (int i = 0; i < levels; i++)
            {
                while (prev.Levels <= i) prev = prev.GetPrev(i - 1);
                while (next.Levels <= i) next = next.GetNext(i - 1);
                node.SetPrev(i, prev);
                node.SetNext(i, next);

                prev.SetNext(i, node);
                next.SetPrev(i, node);
            }
        }

        private int CompareNode(SkipListNode node, TKey key)
        {
            if (node is SkipListHeadNode) return -1;
            if (node is SkipListTailNode) return 1;

            return _nodeComparer.Compare(node.Key, key);
        }

        [DebuggerDisplay("Node [{Key},{Value}] (lv:{_next.Length})")]
        internal class SkipListNode : Node<TKey, TValue>
        {
            protected SkipListNode[] _next;
            protected SkipListNode[] _prev;

            protected internal SkipListNode(TKey key, TValue value, int levels) : base(key, value)
            {
                _next = new SkipListNode[levels];
                _prev = new SkipListNode[levels];
            }

            internal virtual int Levels { get { return _next.Length; } }

            public virtual SkipListNode GetNext(int level)
            {
                return _next[level];
            }

            public virtual void SetNext(int level, SkipListNode node)
            {
                if (level < _next.Length) _next[level] = node;
            }

            public virtual void SetPrev(int level, SkipListNode node)
            {
                if (level < _prev.Length) _prev[level] = node;
            }

            public virtual SkipListNode GetPrev(int level)
            {
                return _prev[level];
            }

            public override string ToString()
            {
                return string.Format("Node [{0},{1}] (lv {2})", Key, Value, Levels);
            }
        }

        [DebuggerDisplay("Head (lv {_next.Length})")]
        internal class SkipListHeadNode : SkipListNode
        {
            protected internal SkipListHeadNode() : base(default(TKey), default(TValue), 1)
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

            internal void LevelsUp(int newLevels)
            {
                var newNext = new SkipListNode[newLevels];
                Array.Copy(_next, newNext, _next.Length);
                _next = newNext;
            }

            public override string ToString()
            {
                return string.Format("Head (lv {0})", Levels);
            }
        }

        [DebuggerDisplay("Tail (lv {_prev.Length})")]
        internal class SkipListTailNode : SkipListNode
        {
            protected internal SkipListTailNode() : base(default(TKey), default(TValue), 1)
            {
            }

            internal override int Levels { get { return _prev.Length; } }

            public override SkipListNode GetNext(int level)
            {
                throw new InvalidOperationException("There is no element following tail.");
            }

            public override void SetNext(int level, SkipListNode node)
            {
                throw new InvalidOperationException("Not allowed to set element following tail.");
            }

            internal void LevelsUp(int newLevels)
            {
                var newPrev = new SkipListNode[newLevels];
                Array.Copy(_prev, newPrev, _prev.Length);
                _prev = newPrev;
            }

            public override string ToString()
            {
                return string.Format("Tail (lv {0})", Levels);
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
