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
    internal struct CanonicalEquatableStruct : IEquatable<CanonicalEquatableStruct>
    {
        // private fields
        private int _x;
        private int _y;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CanonicalEquatableStruct"/> struct.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public CanonicalEquatableStruct(int x, int y)
        {
            _x = y;
            _y = y;
        }

        // public operators
        /// <summary>
        /// Determines whether two <see cref="CanonicalEquatableStruct"/> instances are equal.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        ///   <c>true</c> if the left hand side is equal to the right hand side; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(CanonicalEquatableStruct lhs, CanonicalEquatableStruct rhs)
        {
            return lhs.Equals(rhs);
        }

        /// <summary>
        /// Determines whether two <see cref="CanonicalEquatableStruct"/> instances are not equal.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        ///   <c>true</c> if the left hand side is not equal to the right hand side; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(CanonicalEquatableStruct lhs, CanonicalEquatableStruct rhs)
        {
            return !(lhs == rhs);
        }

        // public methods
         /// <summary>
        /// Determines whether the specified <see cref="CanonicalEquatableStruct" /> is equal to this instance.
        /// </summary>
        /// <param name="rhs">The <see cref="CanonicalEquatableStruct" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="CanonicalEquatableStruct" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(CanonicalEquatableStruct rhs)
        {
           // actual work done here to avoid boxing
            // be sure x and y implement ==, otherwise use Equals
            return
                _x == rhs._x && 
                _y == rhs._y;
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
            if (!(obj is CanonicalEquatableStruct)) { return false; } // handles obj == null correctly
            return Equals((CanonicalEquatableStruct)obj);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(_x)
                .Hash(_y)
                .GetHashCode();
        }
    }
}
