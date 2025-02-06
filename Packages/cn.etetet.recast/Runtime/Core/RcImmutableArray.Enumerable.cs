using System;
using System.Collections;
using System.Collections.Generic;

namespace DotRecast.Core
{
    public readonly partial struct RcImmutableArray<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            return EnumeratorObject.Create(_array);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return EnumeratorObject.Create(_array);
        }

        private sealed class EnumeratorObject : IEnumerator<T>
        {
            private static readonly IEnumerator<T> EmptyEnumerator = new EnumeratorObject(Empty._array!);
            private readonly T[] _array;
            private int _index;

            internal static IEnumerator<T> Create(T[] array)
            {
                if (array.Length != 0)
                {
                    return new EnumeratorObject(array);
                }
                else
                {
                    return EmptyEnumerator;
                }
            }


            private EnumeratorObject(T[] array)
            {
                _index = -1;
                _array = array;
            }

            public T Current
            {
                get
                {
                    if (unchecked((uint)_index) < (uint)_array.Length)
                    {
                        return _array[_index];
                    }

                    throw new InvalidOperationException();
                }
            }

            object IEnumerator.Current => this.Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                int newIndex = _index + 1;
                int length = _array.Length;

                if ((uint)newIndex <= (uint)length)
                {
                    _index = newIndex;
                    return (uint)newIndex < (uint)length;
                }

                return false;
            }

            void IEnumerator.Reset()
            {
                _index = -1;
            }
        }
    }
}