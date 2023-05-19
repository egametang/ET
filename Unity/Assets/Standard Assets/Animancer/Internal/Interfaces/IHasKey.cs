// Animancer // https://kybernetik.com.au/animancer // Copyright 2020 Kybernetik //

namespace Animancer
{
    /// <summary>Exposes a <see cref="Key"/> object that can be used for dictionaries and hash sets.</summary>
    /// <remarks>
    /// Documentation: <see href="https://kybernetik.com.au/animancer/docs/manual/playing/states#keys">Keys</see>
    /// </remarks>
    /// https://kybernetik.com.au/animancer/api/Animancer/IHasKey
    /// 
    public interface IHasKey
    {
        /************************************************************************************************************************/

        /// <summary>An identifier object that can be used for dictionaries and hash sets.</summary>
        object Key { get; }

        /************************************************************************************************************************/
    }
}

