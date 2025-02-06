namespace DotRecast.Core
{
    public readonly partial struct RcImmutableArray<T>
    {
#pragma warning disable CA1825
        public static readonly RcImmutableArray<T> Empty = new RcImmutableArray<T>(new T[0]);
#pragma warning restore CA1825

        private readonly T[] _array;

        internal RcImmutableArray(T[] items)
        {
            _array = items;
        }

        public T this[int index] => _array![index];
        public int Length => _array!.Length;
    }
}