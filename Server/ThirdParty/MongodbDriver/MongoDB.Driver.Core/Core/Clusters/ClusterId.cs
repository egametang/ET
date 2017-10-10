/* Copyright 2013-2015 MongoDB Inc.
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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Clusters
{
    /// <summary>
    /// Represents a cluster identifier.
    /// </summary>
#if NET45
    [Serializable]
#endif
    public sealed class ClusterId : IEquatable<ClusterId>
    {
        // fields
        private readonly int _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterId"/> class.
        /// </summary>
        public ClusterId()
            : this(IdGenerator<ClusterId>.GetNextId())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ClusterId"/> class.
        /// </summary>
        /// <param name="value">The value.</param>
        public ClusterId(int value)
        {
            _value = value;
        }

        // properties
        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public int Value
        {
            get { return _value; }
        }

        // methods
        /// <inheritdoc/>
        public bool Equals(ClusterId other)
        {
            if (other == null)
            {
                return false;
            }
            return _value == other._value;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as ClusterId);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _value.ToString();
        }
    }
}
