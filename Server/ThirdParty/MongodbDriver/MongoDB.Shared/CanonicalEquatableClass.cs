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
    internal class CanonicalEquatableClass : IEquatable<CanonicalEquatableClass>
    {
        // private fields
        private int _x;
        private int _y;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CanonicalEquatableClass"/> class.
        /// </summary>
        /// <param name="x">The x.</param>
        /// <param name="y">The y.</param>
        public CanonicalEquatableClass(int x, int y)
        {
            _x = y;
            _y = y;
        }

        // public operators
        /// <summary>
        /// Determines whether two <see cref="CanonicalEquatableClass"/> instances are equal.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        ///   <c>true</c> if the left hand side is equal to the right hand side; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(CanonicalEquatableClass lhs, CanonicalEquatableClass rhs)
        {
            return object.Equals(lhs, rhs); // handles lhs == null correctly
        }

        /// <summary>
        /// Determines whether two <see cref="CanonicalEquatableClass"/> instances are not equal.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        ///   <c>true</c> if the left hand side is not equal to the right hand side; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(CanonicalEquatableClass lhs, CanonicalEquatableClass rhs)
        {
            return !(lhs == rhs);
        }

        // public methods
        /// <summary>
        /// Determines whether the specified <see cref="CanonicalEquatableClass" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="CanonicalEquatableClass" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="CanonicalEquatableClass" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(CanonicalEquatableClass obj)
        {
            return Equals((object)obj); // handles obj == null correctly
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
            // actual work done here (in virtual Equals) to handle inheritance
            // use ReferenceEquals consistently because sometimes using == can lead to recursion loops
            // make sure to use GetType instead of typeof in case derived classes are involved
            if (object.ReferenceEquals(obj, null) || GetType() != obj.GetType()) { return false; }
            var rhs = (CanonicalEquatableClass)obj;
            return // be sure x and y implement ==, otherwise use Equals
                _x == rhs._x &&
                _y == rhs._y;
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
