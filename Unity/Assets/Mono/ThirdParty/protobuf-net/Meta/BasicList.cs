using System;
using System.Collections;

namespace ProtoBuf.Meta
{

    internal sealed class MutableList : BasicList
    {
        /*  Like BasicList, but allows existing values to be changed
         */ 
        public new object this[int index] {
            get { return head[index]; }
            set { head[index] = value; }
        }
        public void RemoveLast()
        {
            head.RemoveLastWithMutate();
        }

        public void Clear()
        {
            head.Clear();
        }
    }
    internal class BasicList : IEnumerable
    {
        /* Requirements:
         *   - Fast access by index
         *   - Immutable in the tail, so a node can be read (iterated) without locking
         *   - Lock-free tail handling must match the memory mode; struct for Node
         *     wouldn't work as "read" would not be atomic
         *   - Only operation required is append, but this shouldn't go out of its
         *     way to be inefficient
         *   - Assume that the caller is handling thread-safety (to co-ordinate with
         *     other code); no attempt to be thread-safe
         *   - Assume that the data is private; internal data structure is allowed to
         *     be mutable (i.e. array is fine as long as we don't screw it up)
         */
        private static readonly Node nil = new Node(null, 0);
        public void CopyTo(Array array, int offset)
        {
            head.CopyTo(array, offset);
        }
        protected Node head = nil;
        public int Add(object value)
        {
            return (head = head.Append(value)).Length - 1;
        }
        public object this[int index] { get { return head[index]; } }
        //public object TryGet(int index)
        //{
        //    return head.TryGet(index);
        //}
        public void Trim() { head = head.Trim(); }
        public int Count { get { return head.Length; } }
        IEnumerator IEnumerable.GetEnumerator() { return new NodeEnumerator(head); }
        public NodeEnumerator GetEnumerator() { return new NodeEnumerator(head); }

        public struct NodeEnumerator : IEnumerator
        {
            private int position;
            private readonly Node node;
            internal NodeEnumerator(Node node)
            {
                this.position = -1;
                this.node = node;
            }
            void IEnumerator.Reset() { position = -1; }
            public object Current { get { return node[position]; } }
            public bool MoveNext()
            {
                int len = node.Length;
                return (position <= len) && (++position < len);
            }
        }
        internal sealed class Node
        {
            public object this[int index]
            {
                get {
                    if (index >= 0 && index < length)
                    {
                        return data[index];
                    }
                    throw new ArgumentOutOfRangeException("index");
                }
                set
                {
                    if (index >= 0 && index < length)
                    {
                        data[index] = value;
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException("index");
                    }
                }
            }
            //public object TryGet(int index)
            //{
            //    return (index >= 0 && index < length) ? data[index] : null;
            //}
            private readonly object[] data;
            
            private int length;
            public int Length { get { return length; } }
            internal Node(object[] data, int length)
            {
                Helpers.DebugAssert((data == null && length == 0) ||
                    (data != null && length > 0 && length <= data.Length));
                this.data = data;

                this.length = length;
            }
            public void RemoveLastWithMutate()
            {
                if (length == 0) throw new InvalidOperationException();
                length -= 1;
            }
            public Node Append(object value)
            {
                object[] newData;
                int newLength = length + 1;
                if (data == null)
                {
                    newData = new object[10];
                }
                else if (length == data.Length)
                {
                    newData = new object[data.Length * 2];
                    Array.Copy(data, newData, length);
                } else
                {
                    newData = data;
                }
                newData[length] = value;
                return new Node(newData, newLength);
            }
            public Node Trim()
            {
                if (length == 0 || length == data.Length) return this;
                object[] newData = new object[length];
                Array.Copy(data, newData, length);
                return new Node(newData, length);
            }

            internal int IndexOfString(string value)
            {
                for (int i = 0; i < length; i++)
                {
                    if ((string)value == (string)data[i]) return i;
                }
                return -1;
            }
            internal int IndexOfReference(object instance)
            {
                for (int i = 0; i < length; i++)
                {
                    if ((object)instance == (object)data[i]) return i;
                } // ^^^ (object) above should be preserved, even if this was typed; needs
                  // to be a reference check
                return -1;
            }
            internal int IndexOf(MatchPredicate predicate, object ctx)
            {
                for (int i = 0; i < length; i++)
                {
                    if (predicate(data[i], ctx)) return i;
                }
                return -1;
            }

            internal void CopyTo(Array array, int offset)
            {
                if (length > 0)
                {
                    Array.Copy(data, 0, array, offset, length);
                }
            }

            internal void Clear()
            {
                if(data != null)
                {
                    Array.Clear(data, 0, data.Length);
                }
                length = 0;
            }
        }

        internal int IndexOf(MatchPredicate predicate, object ctx)
        {
            return head.IndexOf(predicate, ctx);
        }
        internal int IndexOfString(string value)
        {
            return head.IndexOfString(value);
        }
        internal int IndexOfReference(object instance)
        {
            return head.IndexOfReference(instance);
        }

        internal delegate bool MatchPredicate(object value, object ctx);

        internal bool Contains(object value)
        {
            foreach (object obj in this)
            {
                if (object.Equals(obj, value)) return true;
            }
            return false;
        }
        internal sealed class Group
        {
            public readonly int First;
            public readonly BasicList Items;
            public Group(int first)
            {
                this.First = first;
                this.Items = new BasicList();
            }
        }
        internal static BasicList GetContiguousGroups(int[] keys, object[] values)
        {
            if (keys == null) throw new ArgumentNullException("keys");
            if (values == null) throw new ArgumentNullException("values");
            if (values.Length < keys.Length) throw new ArgumentException("Not all keys are covered by values", "values");
            BasicList outer = new BasicList();
            Group group = null;
            for (int i = 0; i < keys.Length; i++)
            {
                if (i == 0 || keys[i] != keys[i - 1]) { group = null; }
                if (group == null)
                {
                    group = new Group(keys[i]);
                    outer.Add(group);
                }
                group.Items.Add(values[i]);
            }
            return outer;
        }
    }


}