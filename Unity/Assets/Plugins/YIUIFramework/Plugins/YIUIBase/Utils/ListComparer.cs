using System.Collections.Generic;
using System.Linq;

namespace YIUIFramework
{
    /// <summary>
    /// The list comparer.
    /// </summary>
    public sealed class ListComparer<T> : IEqualityComparer<List<T>>
    {
        private static volatile ListComparer<T> defaultComparer;

        /// <summary>
        /// Gets a default instance of the <see cref="ListComparer{T}"/>.
        /// </summary>
        public static ListComparer<T> Default
        {
            get
            {
                if (defaultComparer == null)
                {
                    defaultComparer = new ListComparer<T>();
                }

                return defaultComparer;
            }
        }

        /// <inheritdoc />
        public bool Equals(List<T> x, List<T> y)
        {
            return x.SequenceEqual(y);
        }

        /// <inheritdoc />
        public int GetHashCode(List<T> obj)
        {
            int hashcode = 0;
            foreach (T t in obj)
            {
                hashcode ^= t.GetHashCode();
            }

            return hashcode;
        }
    }
}