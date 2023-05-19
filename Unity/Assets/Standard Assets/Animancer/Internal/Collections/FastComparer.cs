// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

using System.Collections.Generic;

namespace Animancer
{
    /// <summary>
    /// An <see cref="IEqualityComparer{T}"/> which ignores overloaded equality operators so it is faster than
    /// <see cref="EqualityComparer{T}.Default"/> for types derived from <see cref="UnityEngine.Object"/>.
    /// </summary>
    /// <remarks>
    /// This class is used when <see cref="AnimancerPlayable.StateDictionary.ReferenceKeysOnly"/> is false.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states#performance">Performance</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/FastComparer
    /// 
    public sealed class FastComparer : IEqualityComparer<object>
    {
        /************************************************************************************************************************/

        /// <summary>Singleton instance.</summary>
        public static readonly FastComparer Instance = new FastComparer();

        /// <summary>Calls <see cref="object.Equals(object, object)"/>.</summary>
        bool IEqualityComparer<object>.Equals(object x, object y) => Equals(x, y);

        /// <summary>Calls <see cref="object.GetHashCode"/>.</summary>
        int IEqualityComparer<object>.GetHashCode(object obj) => obj.GetHashCode();

        /************************************************************************************************************************/
    }

    /// <summary>
    /// An <see cref="IEqualityComparer{T}"/> which uses <see cref="object.ReferenceEquals"/> to be even faster than
    /// <see cref="FastComparer"/>. Unfortunately this means it will not work for boxed value types (such as enums).
    /// </summary>
    /// <remarks>
    /// This class is used when <see cref="AnimancerPlayable.StateDictionary.ReferenceKeysOnly"/> is true.
    /// <para></para>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states#performance">Performance</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/FastReferenceComparer
    /// 
    public sealed class FastReferenceComparer : IEqualityComparer<object>
    {
        /************************************************************************************************************************/

        /// <summary>Singleton instance.</summary>
        public static readonly FastReferenceComparer Instance = new FastReferenceComparer();

        /// <summary>Calls <see cref="object.ReferenceEquals"/>.</summary>
        bool IEqualityComparer<object>.Equals(object x, object y) => ReferenceEquals(x, y);

        /// <summary>Calls <see cref="object.GetHashCode"/>.</summary>
        int IEqualityComparer<object>.GetHashCode(object obj) => obj.GetHashCode();

        /************************************************************************************************************************/
    }
}

