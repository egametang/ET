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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a database namespace.
    /// </summary>
    public sealed class DatabaseNamespace : IEquatable<DatabaseNamespace>
    {
        // static fields
        private static readonly DatabaseNamespace __admin = new DatabaseNamespace("admin");

        // static properties
        /// <summary>
        /// Gets the admin database namespace.
        /// </summary>
        /// <value>
        /// The admin database namespace.
        /// </value>
        public static DatabaseNamespace Admin
        {
            get { return __admin; }
        }

        // static methods
        /// <summary>
        /// Determines whether the specified database name is valid.
        /// </summary>
        /// <param name="name">The database name.</param>
        /// <returns>True if the database name is valid.</returns>
        public static bool IsValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return false;
            }

            var index = name.IndexOfAny(new[] { '\0', '.' });
            return index == -1;
        }

        // fields
        private readonly string _databaseName;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DatabaseNamespace"/> class.
        /// </summary>
        /// <param name="databaseName">The name of the database.</param>
        public DatabaseNamespace(string databaseName)
        {
            Ensure.IsNotNull(databaseName, nameof(databaseName));
            _databaseName = Ensure.That(databaseName, IsValid, nameof(databaseName), "Database names must be non-empty and not contain '.' or the null character.");
        }

        // properties
        internal CollectionNamespace CommandCollection
        {
            get { return new CollectionNamespace(this, "$cmd"); }
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        /// <value>
        /// The name of the database.
        /// </value>
        public string DatabaseName
        {
            get { return _databaseName; }
        }

        internal CollectionNamespace SystemIndexesCollection
        {
            get { return new CollectionNamespace(this, "system.indexes"); }
        }

        internal CollectionNamespace SystemNamespacesCollection
        {
            get { return new CollectionNamespace(this, "system.namespaces"); }
        }

        // methods
        /// <inheritdoc/>
        public bool Equals(DatabaseNamespace other)
        {
            if (other == null)
            {
                return false;
            }

            return _databaseName == other._databaseName;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as DatabaseNamespace);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _databaseName.GetHashCode();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _databaseName;
        }
    }
}