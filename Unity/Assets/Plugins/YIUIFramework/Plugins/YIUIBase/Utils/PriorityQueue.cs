using System;
using System.Collections;
using System.Collections.Generic;

namespace YIUIFramework
{
    /// <summary>
    /// The priority queue is a max-heap used to find the maximum value.
    /// </summary>
    public sealed class PriorityQueue<T> : IEnumerable<T>
    {
        private IComparer<T> comparer;
        private T[]          heap;
        private HashSet<T>   fastFinder = new HashSet<T>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueue{T}"/> 
        /// class.
        /// </summary>
        public PriorityQueue()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueue{T}"/> 
        /// class, with specify capacity.
        /// </summary>
        public PriorityQueue(int capacity)
            : this(capacity, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueue{T}"/> 
        /// class, with specify comparer.
        /// </summary>
        public PriorityQueue(IComparer<T> comparer)
            : this(16, comparer)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriorityQueue{T}"/> 
        /// class, with specify capacity and comparer.
        /// </summary>
        public PriorityQueue(int capacity, IComparer<T> comparer)
        {
            this.comparer = (comparer == null) ? Comparer<T>.Default : comparer;
            this.heap     = new T[capacity];
        }

        /// <summary>
        /// Gets the count in this queue.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets the object at specify index.
        /// </summary>
        public T this[int index]
        {
            get { return this.heap[index]; }
        }

        public bool Contains(T v)
        {
            return fastFinder.Contains(v);
        }

        /// <summary>
        /// Get the enumerator for this item.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this.heap, this.Count);
        }

        /// <summary>
        /// Get the enumerator for this item.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Clear this container.
        /// </summary>
        public void Clear()
        {
            this.Count = 0;
            this.fastFinder.Clear();
        }

        /// <summary>
        /// Push a new value into this queue.
        /// </summary>
        public void Push(T v)
        {
            if (this.Count >= this.heap.Length)
            {
                Array.Resize(ref this.heap, this.Count * 2);
            }

            this.heap[this.Count] = v;
            this.SiftUp(this.Count++);
            fastFinder.Add(v);
        }

        /// <summary>
        /// Pop the max value out of this queue.
        /// </summary>
        public T Pop()
        {
            var v = this.Top();
            this.heap[0] = this.heap[--this.Count];
            if (this.Count > 0)
            {
                this.SiftDown(0);
            }

            fastFinder.Remove(v);
            return v;
        }

        /// <summary>
        /// Access the max value in this queue.
        /// </summary>
        public T Top()
        {
            if (this.Count > 0)
            {
                return this.heap[0];
            }

            throw new InvalidOperationException("The PriorityQueue is empty.");
        }

        private void SiftUp(int n)
        {
            var v = this.heap[n];
            for (var n2 = n / 2;
                 n > 0 && this.comparer.Compare(v, this.heap[n2]) > 0;
                 n = n2, n2 /= 2)
            {
                this.heap[n] = this.heap[n2];
            }

            this.heap[n] = v;
        }

        private void SiftDown(int n)
        {
            var v = this.heap[n];
            for (var n2 = n * 2; n2 < this.Count; n = n2, n2 *= 2)
            {
                if (n2 + 1 < this.Count &&
                    this.comparer.Compare(this.heap[n2 + 1], this.heap[n2]) > 0)
                {
                    ++n2;
                }

                if (this.comparer.Compare(v, this.heap[n2]) >= 0)
                {
                    break;
                }

                this.heap[n] = this.heap[n2];
            }

            this.heap[n] = v;
        }

        /// <summary>
        /// The enumerator for this <see cref="PriorityQueue{T}"/>.
        /// </summary>
        public struct Enumerator : IEnumerator<T>
        {
            private readonly T[] heap;
            private readonly int count;
            private          int index;

            /// <summary>
            /// Initializes a new instance of the <see cref="Enumerator"/> 
            /// struct.
            /// </summary>
            internal Enumerator(T[] heap, int count)
            {
                this.heap  = heap;
                this.count = count;
                this.index = -1;
            }

            /// <inheritdoc/>
            public T Current
            {
                get { return this.heap[this.index]; }
            }

            /// <inheritdoc/>
            object IEnumerator.Current
            {
                get { return this.Current; }
            }

            /// <inheritdoc/>
            public void Dispose()
            {
            }

            /// <inheritdoc/>
            public void Reset()
            {
                this.index = -1;
            }

            /// <inheritdoc/>
            public bool MoveNext()
            {
                return (this.index <= this.count) &&
                    (++this.index < this.count);
            }
        }
    }
}