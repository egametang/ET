/* Copyright 2010-2016 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a fast converter from integer indexes to UTF8 BSON array element names.
    /// </summary>
    internal interface IArrayElementNameAccelerator
    {
        /// <summary>
        /// Gets the element name bytes.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>The element name bytes.</returns>
        byte[] GetElementNameBytes(int index);
    }

    /// <summary>
    /// Represents a fast converter from integer indexes to UTF8 BSON array element names.
    /// </summary>
    internal class ArrayElementNameAccelerator : IArrayElementNameAccelerator
    {
        #region static
        // static fields
        private static IArrayElementNameAccelerator __default = new ArrayElementNameAccelerator(1000);

        // static properties
        /// <summary>
        /// Gets or sets the default array element name accelerator.
        /// </summary>
        public static IArrayElementNameAccelerator Default
        {
            get { return __default; }
            set { __default = value; }
        }
        #endregion

        // fields
        private readonly byte[][] _cachedElementNames;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ArrayElementNameAccelerator"/> class.
        /// </summary>
        /// <param name="numberOfCachedElementNames">The number of cached element names.</param>
        public ArrayElementNameAccelerator(int numberOfCachedElementNames)
        {
            _cachedElementNames = new byte[numberOfCachedElementNames][];
            for (int index = 0; index < numberOfCachedElementNames; index++)
            {
                _cachedElementNames[index] = CreateElementNameBytes(index);
            }
        }

        // methods
        private byte[] CreateElementNameBytes(int index)
        {
            // unrolled loop optimized for index values >= 1000 and < 10,000

            const int asciiZero = 48;

            var n = index;
            var a = (byte)(asciiZero + n % 10);
            n = n / 10;
            var b = (byte)(asciiZero + n % 10);
            n = n / 10;
            var c = (byte)(asciiZero + n % 10);
            n = n / 10;
            var d = (byte)(asciiZero + n % 10);
            n = n / 10;

            if (n == 0)
            {
                if (d != (byte)asciiZero) { return new[] { d, c, b, a }; }
                if (c != (byte)asciiZero) { return new[] { c, b, a }; }
                if (b != (byte)asciiZero) { return new[] { b, a }; }
                return new[] { a };
            }

            var e = (byte)(asciiZero + n % 10);
            n = n / 10;
            if (n == 0) { return new[] { e, d, c, b, a }; }

            // really large indexes should be extremely rare and not worth optimizing further
            return Utf8Encodings.Strict.GetBytes(index.ToString());
        }

        /// <summary>
        /// Gets the element name bytes.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>
        /// The element name bytes.
        /// </returns>
        public byte[] GetElementNameBytes(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException("index", "index is negative.");
            }

            if (index < _cachedElementNames.Length)
            {
                return  _cachedElementNames[index];
            }

            return CreateElementNameBytes(index);
        }
    }
}
