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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a list of credentials and the rules about how credentials can be used together.
    /// </summary>
    internal class MongoCredentialStore : IEnumerable<MongoCredential>, IEquatable<MongoCredentialStore>
    {
        // private fields
        private readonly ReadOnlyCollection<MongoCredential> _credentials;

        // constructors
        /// <summary>
        /// Creates a new instance of the MongoCredentialStore class.
        /// </summary>
        /// <param name="credentials">The credentials.</param>
        public MongoCredentialStore(IEnumerable<MongoCredential> credentials)
        {
            if (credentials == null)
            {
                throw new ArgumentNullException("credentials");
            }
            _credentials = new ReadOnlyCollection<MongoCredential>(new List<MongoCredential>(credentials));
            EnsureCredentialsAreCompatibleWithEachOther();
        }

        // public operators
        /// <summary>
        /// Determines whether two <see cref="MongoCredentialStore"/> instances are equal.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        ///   <c>true</c> if the left hand side is equal to the right hand side; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(MongoCredentialStore lhs, MongoCredentialStore rhs)
        {
            return object.Equals(lhs, rhs); // handles lhs == null correctly
        }

        /// <summary>
        /// Determines whether two <see cref="MongoCredentialStore"/> instances are not equal.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        ///   <c>true</c> if the left hand side is not equal to the right hand side; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(MongoCredentialStore lhs, MongoCredentialStore rhs)
        {
            return !(lhs == rhs);
        }

        // public methods
        /// <summary>
        /// Determines whether the specified <see cref="MongoCredentialStore" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="MongoCredentialStore" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="MongoCredentialStore" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(MongoCredentialStore obj)
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
            if (object.ReferenceEquals(obj, null) || GetType() != obj.GetType()) { return false; }
            var rhs = (MongoCredentialStore)obj;
            return _credentials.SequenceEqual(rhs._credentials);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<MongoCredential> GetEnumerator()
        {
            return _credentials.GetEnumerator();
        }

        /// <summary>
        /// Gets the hashcode for the credential store.
        /// </summary>
        /// <returns>The hashcode.</returns>
        public override int GetHashCode()
        {
            // see Effective Java by Joshua Bloch
            int hash = 17;
            foreach (var credential in _credentials)
            {
                hash = 37 * hash + credential.GetHashCode();
            }
            return hash;
        }

        /// <summary>
        /// Returns a string representation of the credential store.
        /// </summary>
        /// <returns>A string representation of the credential store.</returns>
        public override string ToString()
        {
            return string.Format("{{{0}}}", string.Join(",", _credentials.Select(c => c.ToString()).ToArray()));
        }

        // explicit interface implementations
        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
        /// </returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _credentials.GetEnumerator();
        }

        // private metods
        private void EnsureCredentialsAreCompatibleWithEachOther()
        {
            var sources = new HashSet<string>(_credentials.Select(c => c.Source));
            if (sources.Count < _credentials.Count)
            {
                throw new ArgumentException("The server requires that each credential provided be from a different source.");
            }
        }
    }
}
