using System.Collections;
using System.Collections.Generic;

namespace ET
{
#if DOTNET
    public class SortedCollection<K, V>: SortedDictionary<K, V>
    {
    }
    
#else
    public class SortedCollection<K, V>: IEnumerable<KeyValuePair<K, V>>
    {
        private class Enumerator: IEnumerator<KeyValuePair<K, V>>
        {
            private readonly SortedCollection<K, V> sortedCollection;
            private Node current;
            private int index;

            public Enumerator(SortedCollection<K, V> sortedCollection)
            {
                this.sortedCollection = sortedCollection;
                this.index = 0;
                this.current = null;
            }
            
            public void Dispose()
            {
                // TODO release managed resources here
            }

            public bool MoveNext()
            {
                if (++this.index > this.sortedCollection.dict.Count)
                {
                    return false;
                }
                if (this.current == null)
                {
                    this.current = this.sortedCollection.head;
                    return true;
                }
                if (this.current.Next == null)
                {
                    return false;
                }
                this.current = this.current.Next;
                return true;
            }

            public void Reset()
            {
                this.index = 0;
                this.current = null;
            }

            public KeyValuePair<K, V> Current
            {
                get
                {
                    return new KeyValuePair<K, V>(this.current.Key, this.current.Value);
                }
            }

            object IEnumerator.Current => this.Current;
        }
        
        public class Node
        {
            internal Node Next;
            internal Node Pre;
            public K Key;
            public V Value;
        }

        private readonly IComparer<K> comparer;

        private readonly Dictionary<K, Node> dict = new();
        
        private readonly Queue<Node> pool = new();

        private Node head;

        private readonly Enumerator enumerator;

        public SortedCollection()
        {
            this.comparer = Comparer<K>.Default;
            this.enumerator = new Enumerator(this);
        }
        
        public SortedCollection(IComparer<K> comparer)
        {
            this.comparer = comparer ?? Comparer<K>.Default;
            this.enumerator = new Enumerator(this);
        }

        private Node Fetch()
        {
            if (this.pool.Count == 0)
            {
                return new Node();
            }
            return this.pool.Dequeue();
        }
        
        private void Recycle(Node node)
        {
            node.Next = null;
            node.Pre = null;
            node.Key = default;
            node.Value = default;
            if (this.pool.Count > 1000)
            {
                return;
            }
            this.pool.Enqueue(node);
        }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator()
        {
            this.enumerator.Reset();
            return this.enumerator;
        }

        public void Add(K k, V v)
        {
            Node node = this.Fetch();
            node.Key = k;
            node.Value = v;
            this.dict.Add(k, node);
            
            if (this.head == null)
            {
                this.head = node;
                return;
            }
            
            Node p = this.head;
            if (comparer.Compare(k, p.Key) < 0)
            {
                node.Next = p;
                p.Pre = node;
                this.head = node;
                return;
            }

            Node q = p.Next;
            while (true)
            {
                if (q == null) // 到末尾
                {
                    p.Next = node;
                    node.Pre = p;
                    break;
                }
                
                int ret = comparer.Compare(k, q.Key);

                if (ret == 0)
                {
                    break;
                }
                
                if (ret < 0)
                {
                    node.Next = q;
                    q.Pre = node;
                    p.Next = node;
                    node.Pre = p;
                    break;
                }
                
                p = p.Next;
                q = q.Next;
            }
        }

        public bool Remove(K k)
        {
            if (!this.dict.Remove(k, out Node node))
            {
                return false;
            }

            if (this.dict.Count == 0)
            {
                this.head = null;
                this.Recycle(node);
                return true;
            }

            if (node.Pre == null)
            {
                node.Next.Pre = null;
                this.head = node.Next;
                this.Recycle(node);
                return true;
            }

            if (node.Next == null)
            {
                node.Pre.Next = null;
                this.Recycle(node);
                return true;
            }

            node.Pre.Next = node.Next;
            node.Next.Pre = node.Pre;
            this.Recycle(node);
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public V this[K k]
        {
            get
            {
                return this.dict[k].Value;
            }
            set
            {
                this.Remove(k);
                this.Add(k, value);
            }
        }

        public bool TryGetValue(K k, out V v)
        {
            bool ret = this.dict.TryGetValue(k, out Node node);
            v = ret? node.Value : default;
            return ret;
        }

        public int Count
        {
            get
            {
                return this.dict.Count;
            }
        }
    }
#endif
}
