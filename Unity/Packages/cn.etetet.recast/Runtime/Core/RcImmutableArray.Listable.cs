using System;
using System.Collections.Generic;

namespace DotRecast.Core
{
    public readonly partial struct RcImmutableArray<T> : IList<T>
    {
        public int Count => Length;
        public bool IsReadOnly => true;

        T IList<T>.this[int index]
        {
            get
            {
                var self = this;
                return self[index];
            }
            set => throw new NotSupportedException();
        }


        public int IndexOf(T item)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (_array![i].Equals(item))
                    return i;
            }

            return -1;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            var self = this;
            Array.Copy(self._array!, 0, array, arrayIndex, self.Length);
        }

        public void Add(T item)
        {
            throw new NotSupportedException();
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Remove(T item)
        {
            throw new NotSupportedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotSupportedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException();
        }
    }
}