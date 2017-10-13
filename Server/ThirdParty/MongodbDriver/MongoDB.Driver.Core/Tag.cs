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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;
using MongoDB.Shared;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a replica set member tag.
    /// </summary>
    public sealed class Tag : IEquatable<Tag>
    {
        // fields
        private readonly string _name;
        private readonly string _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public Tag(string name, string value)
        {
            _name = Ensure.IsNotNull(name, nameof(name));
            _value = Ensure.IsNotNull(value, nameof(value));
        }

        // properties
        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
        {
            get { return _value; }
        }

        // methods
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as Tag);
        }

        /// <inheritdoc/>
        public bool Equals(Tag other)
        {
            if (other == null)
            {
                return false;
            }
            return _name == other._name && _value == other._value;
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(_name)
                .Hash(_value)
                .GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{0} : {1}", _name, _value);
        }
    }
}
