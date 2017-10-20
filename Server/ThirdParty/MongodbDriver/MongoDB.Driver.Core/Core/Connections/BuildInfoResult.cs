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
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Connections
{
    /// <summary>
    /// Represents the result of a buildInfo command.
    /// </summary>
    public sealed class BuildInfoResult : IEquatable<BuildInfoResult>
    {
        // fields
        private readonly BsonDocument _wrapped;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BuildInfoResult"/> class.
        /// </summary>
        /// <param name="wrapped">The wrapped result document.</param>
        public BuildInfoResult(BsonDocument wrapped)
        {
            _wrapped = Ensure.IsNotNull(wrapped, nameof(wrapped));
        }

        // properties
        /// <summary>
        /// Gets the server version.
        /// </summary>
        /// <value>
        /// The server version.
        /// </value>
        public SemanticVersion ServerVersion
        {
            get
            {
                return SemanticVersion.Parse(_wrapped.GetValue("version").ToString());
            }
        }

        /// <summary>
        /// Gets the wrapped result document.
        /// </summary>
        /// <value>
        /// The wrapped result document.
        /// </value>
        public BsonDocument Wrapped
        {
            get
            {
                return _wrapped;
            }
        }

        // methods
        /// <inheritdoc/>
        public bool Equals(BuildInfoResult other)
        {
            if (other == null)
            {
                return false;
            }

            return _wrapped.Equals(other._wrapped);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as BuildInfoResult);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _wrapped.GetHashCode();
        }
    }
}
