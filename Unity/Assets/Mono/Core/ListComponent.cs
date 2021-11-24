using System;
using System.Collections.Generic;

namespace ET
{
    public class ListComponent<T>: IDisposable
    {
        public List<T> List { get; private set; } = new List<T>();

        private static readonly Queue<ListComponent<T>> queue = new Queue<ListComponent<T>>();

        private ListComponent()
        {
        }

        public static ListComponent<T> Create()
        {
            if (queue.Count > 0)
            {
                ListComponent<T> t = queue.Dequeue();
                return t;
            }

            ListComponent<T> listComponent = new ListComponent<T>();
            return listComponent;
        }

        public void Add(T t)
        {
            this.List.Add(t);
        }

        public void Dispose()
        {
            this.List.Clear();
#if NOT_UNITY
            queue.Enqueue(this);
#endif
        }
    }
}