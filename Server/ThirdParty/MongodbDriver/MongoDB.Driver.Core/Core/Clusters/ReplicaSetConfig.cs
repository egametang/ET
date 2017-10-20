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
using System.Net;
using MongoDB.Driver.Core.Misc;
using MongoDB.Shared;

namespace MongoDB.Driver.Core.Clusters
{
    /// <summary>
    /// Represents the config of a replica set (as reported by one of the members of the replica set).
    /// </summary>
    public sealed class ReplicaSetConfig : IEquatable<ReplicaSetConfig>
    {
        #region static
        // static properties
        /// <summary>
        /// Gets an empty replica set config.
        /// </summary>
        /// <value>
        /// An empty replica set config.
        /// </value>
        public static ReplicaSetConfig Empty
        {
            get { return new ReplicaSetConfig(Enumerable.Empty<EndPoint>(), null, null, null); }
        }
        #endregion

        // fields
        private readonly List<EndPoint> _members;
        private readonly string _name;
        private readonly EndPoint _primary;
        private readonly int? _version;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplicaSetConfig"/> class.
        /// </summary>
        /// <param name="members">The members.</param>
        /// <param name="name">The name.</param>
        /// <param name="primary">The primary.</param>
        /// <param name="version">The version.</param>
        public ReplicaSetConfig(
            IEnumerable<EndPoint> members,
            string name,
            EndPoint primary,
            int? version)
        {
            _members = Ensure.IsNotNull(members, nameof(members)).ToList();
            _name = name; // can be null
            _primary = primary; // can be null
            _version = version;
        }

        // properties
        /// <summary>
        /// Gets the members.
        /// </summary>
        /// <value>
        /// The members.
        /// </value>
        public IReadOnlyList<EndPoint> Members
        {
            get { return _members; }
        }

        /// <summary>
        /// Gets the name of the replica set.
        /// </summary>
        /// <value>
        /// The name of the replica set.
        /// </value>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the primary.
        /// </summary>
        /// <value>
        /// The primary.
        /// </value>
        public EndPoint Primary
        {
            get { return _primary; }
        }

        /// <summary>
        /// Gets the replica set config version.
        /// </summary>
        /// <value>
        /// The replica set config version.
        /// </value>
        public int? Version
        {
            get { return _version; }
        }

        // members
        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as ReplicaSetConfig);
        }

        /// <inheritdoc/>
        public bool Equals(ReplicaSetConfig other)
        {
            if (other == null)
            {
                return false;
            }

            return
                _members.SequenceEqual(other._members) &&
                object.Equals(_name, other._name) &&
                object.Equals(_primary, other._primary) &&
                _version.Equals(other._version);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return new Hasher()
                .HashElements(_members)
                .Hash(_name)
                .Hash(_primary)
                .Hash(_version)
                .GetHashCode();
        }
    }
}
