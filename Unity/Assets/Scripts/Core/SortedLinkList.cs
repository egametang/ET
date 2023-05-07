using System;
using System.Collections;
using System.Collections.Generic;

namespace ET
{
    public class SortedLinkList<T, K>: Dictionary<T, K> where T: IComparable<T>, IEnumerable
    {
        //public struct KeyValuePair
        //{
        //    public SortedLinkList<T, K> sortedLinkList;
        //    public T Key;
        //    public K Value;
        //    
        //    public KeyValuePair()
        //}
        
        public class Node
        {
            public Node Next;
            public T Value;
        }

        private readonly Queue<Node> pool = new();

        private Node head;

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
            node.Value = default;
            if (this.pool.Count > 1000)
            {
                return;
            }
            this.pool.Enqueue(node);
        }

        //public new KeyValuePair GetEnumerator()
        //{
        //    return new 
        //}

        public T FirstKey()
        {
            return this.head.Value;
        }

        public new void Add(T t, K k)
        {
            if (this.head == null)
            {
                Node node = this.Fetch();
                node.Value = t;
                this.head = node;
                return;
            }
            if (t.CompareTo(this.head.Value) < 0)
            {
                Node node = this.Fetch();
                node.Value = t;
                node.Next = this.head;
                this.head = node;
                base.Add(t, k);
                return;
            }

            Node p = this.head;
            while (true)
            {
                Node node = null;
                if (p.Next == null)
                {
                    node = this.Fetch();
                    node.Value = t;
                    p.Next = node;
                    break;
                }
                
                int ret = t.CompareTo(p.Next.Value);

                if (ret == 0)
                {
                    break;
                }
                
                if (ret > 0)
                {
                    p = p.Next;
                    continue;
                }

                node = this.Fetch();
                node.Value = t;
                node.Next = p.Next;
                p.Next = node;
            }
            base.Add(t, k);
        }

        public new bool Remove(T t)
        {
            if (this.head == null)
            {
                return false;
            }
            
            Node p = this.head;
            
            while (true)
            {
                if (p.Next == null)
                {
                    break;
                }
                
                int ret = t.CompareTo(p.Next.Value);

                if (ret == 0)
                {
                    this.Recycle(p.Next);
                    p.Next = p.Next.Next;
                    break;
                }
                
                if (ret > 0)
                {
                    p = p.Next;
                }
            }
            return base.Remove(t);
        }
    }
}