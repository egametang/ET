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
using MongoDB.Driver.Core.Misc;
using MongoDB.Shared;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a collection namespace.
    /// </summary>
    public sealed class CollectionNamespace : IEquatable<CollectionNamespace>
    {
        // static methods
        /// <summary>
        /// Creates a new instance of the <see cref="CollectionNamespace"/> class from a collection full name.
        /// </summary>
        /// <param name="fullName">The collection full name.</param>
        /// <returns>A CollectionNamespace.</returns>
        public static CollectionNamespace FromFullName(string fullName)
        {
            Ensure.IsNotNull(fullName, nameof(fullName));

            var index = fullName.IndexOf('.');
            if (index > -1)
            {
                var databaseName = fullName.Substring(0, index);
                var collectionName = fullName.Substring(index + 1);
                return new CollectionNamespace(databaseName, collectionName);
            }
            else
            {
                throw new ArgumentException("Must contain a '.' separating the database name from the collection name.", "fullName");
            }
        }

        /// <summary>
        /// Determines whether the specified collection name is valid.
        /// </summary>
        /// <param name="collectionName">The name of the collection.</param>
        /// <returns>Whether the specified collection name is valid.</returns>
        public static bool IsValid(string collectionName)
        {
            if (string.IsNullOrWhiteSpace(collectionName))
            {
                return false;
            }

            var index = collectionName.IndexOf('\0');
            return index == -1;
        }

        // fields
        private readonly string _collectionName;
        private readonly DatabaseNamespace _databaseNamespace;
        private readonly string _fullName;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionNamespace"/> class.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        /// <param name="collectionName">The name of the collection.</param>
        public CollectionNamespace(string databaseName, string collectionName)
            : this(new DatabaseNamespace(databaseName), collectionName)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionNamespace"/> class.
        /// </summary>
        /// <param name="databaseNamespace">The database namespace.</param>
        /// <param name="collectionName">The name of the collection.</param>
        public CollectionNamespace(DatabaseNamespace databaseNamespace, string collectionName)
        {
            _databaseNamespace = Ensure.IsNotNull(databaseNamespace, nameof(databaseNamespace));
            _collectionName = Ensure.That(collectionName, IsValid, nameof(collectionName), "Collection names must be non-empty and not contain the null character.");
            _fullName = _databaseNamespace.DatabaseName + "." + _collectionName;
        }

        // properties
        /// <summary>
        /// Gets the name of the collection.
        /// </summary>
        /// <value>
        /// The name of the collection.
        /// </value>
        public string CollectionName
        {
            get { return _collectionName; }
        }

        /// <summary>
        /// Gets the database namespace.
        /// </summary>
        /// <value>
        /// The database namespace.
        /// </value>
        public DatabaseNamespace DatabaseNamespace
        {
            get { return _databaseNamespace; }
        }

        /// <summary>
        /// Gets the collection full name.
        /// </summary>
        /// <value>
        /// The collection full name.
        /// </value>
        public string FullName
        {
            get { return _fullName; }
        }

        // methods
        /// <inheritdoc/>
        public bool Equals(CollectionNamespace other)
        {
            if (other == null)
            {
                return false;
            }

            return _databaseNamespace.Equals(other._databaseNamespace) &&
                _collectionName == other._collectionName;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as CollectionNamespace);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return new Hasher()
                .Hash(_databaseNamespace)
                .Hash(_collectionName)
                .GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return FullName;
        }
    }
}