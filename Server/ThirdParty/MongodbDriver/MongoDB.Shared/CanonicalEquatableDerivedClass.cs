/* Copyright 2010-2014 MongoDB Inc.
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

namespace MongoDB.Shared
{
    internal class CanonicalEquatableDerivedClass : CanonicalEquatableClass, IEquatable<CanonicalEquatableDerivedClass>
    {
        // private fields
        private int _z;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CanonicalEquatableDerivedClass"/> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        /// <param name="z">The z.</param>
        public CanonicalEquatableDerivedClass(int x, int y, int z)
            : base(x, y)
        {
            _z = z;
        }

        // base class defines == and !=

        // public methods
        /// <summary>
        /// Determines whether the specified <see cref="CanonicalEquatableDerivedClass" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="CanonicalEquatableDerivedClass" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="CanonicalEquatableDerivedClass" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(CanonicalEquatableDerivedClass obj)
        {
            return Equals((object)obj);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            // base class checks for obj == null and correct type
            if (!base.Equals(obj)) { return false; }
            var rhs = (CanonicalEquatableDerivedClass)obj;
            return // be sure z implements ==, otherwise use Equals
                _z == rhs._z;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            // use hash code of base class as seed to Hasher
            return new Hasher(base.GetHashCode())
                .Hash(_z)
                .GetHashCode();
        }
    }
}
