/* Copyright 2010-2015 MongoDB Inc.
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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents helper methods for use with the <see cref="Optional{T}"/> struct.
    /// </summary>
    public static class Optional
    {
        /// <summary>
        /// Creates an instance of an optional parameter with a value.
        /// </summary>
        /// <remarks>
        /// This helper method can be used when the implicit conversion doesn't work (due to compiler limitations).
        /// </remarks>
        /// <typeparam name="T">The type of the optional parameter.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>An instance of an optional parameter with a value.</returns>
        public static Optional<T> Create<T>(T value)
        {
            return new Optional<T>(value);
        }

        /// <summary>
        /// Creates an instance of an optional parameter with an enumerable value.
        /// </summary>
        /// <typeparam name="TItem">The type of the items of the optional paramater.</typeparam>
        /// <param name="value">The value.</param>
        /// <returns>An instance of an optional parameter with an enumerable value.</returns>
        public static Optional<IEnumerable<TItem>> Enumerable<TItem>(IEnumerable<TItem> value)
        {
            return new Optional<IEnumerable<TItem>>(value);
        }
    }

    /// <summary>
    /// Represents an optional parameter that might or might not have a value.
    /// </summary>
    /// <typeparam name="T">The type of the parameter.</typeparam>
    public struct Optional<T>
    {
        private readonly bool _hasValue;
        private readonly T _value;

        /// <summary>
        /// Initializes a new instance of the <see cref="Optional{T}"/> struct with a value.
        /// </summary>
        /// <param name="value">The value of the parameter.</param>
        public Optional(T value)
        {
            _hasValue = true;
            _value = value;
        }

        /// <summary>
        /// Gets a value indicating whether the optional parameter has a value.
        /// </summary>
        /// <value>
        ///   <c>true</c> if the optional parameter has a value; otherwise, <c>false</c>.
        /// </value>
        public bool HasValue
        {
            get { return _hasValue; }
        }

        /// <summary>
        /// Gets the value of the optional parameter.
        /// </summary>
        /// <value>
        /// The value of the optional parameter.
        /// </value>
        public T Value
        {
            get
            {
                if (!_hasValue)
                {
                    throw new InvalidOperationException("This instance does not have a value.");
                }
                return _value;                
            }
        }

        /// <summary>
        /// Performs an implicit conversion from <see typeparamref="T" /> to an <see cref="Optional{T}" /> with a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }

        /// <summary>
        /// Returns a value indicating whether this optional parameter contains a value that is not equal to an existing value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if this optional parameter contains a value that is not equal to an existing value.</returns>
        public bool Replaces(T value)
        {
            return _hasValue && !object.Equals(_value, value);
        }

        /// <summary>
        /// Returns either the value of this optional parameter if it has a value, otherwise a default value.
        /// </summary>
        /// <param name="value">The default value.</param>
        /// <returns>Either the value of this optional parameter if it has a value, otherwise a default value.</returns>
        public T WithDefault(T value)
        {
            return _hasValue ? _value : value;
        }
    }
}
