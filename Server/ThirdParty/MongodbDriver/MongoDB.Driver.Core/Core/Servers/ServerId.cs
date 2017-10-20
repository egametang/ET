/* Copyright 2013-2016 MongoDB Inc.
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
using System.Net;
#if NET45
using System.Runtime.Serialization;
#endif
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Misc;
using MongoDB.Shared;

namespace MongoDB.Driver.Core.Servers
{
    /// <summary>
    /// Represents a server identifier.
    /// </summary>
#if NET45
    [Serializable]
    public sealed class ServerId : IEquatable<ServerId>, ISerializable
#else
    public sealed class ServerId : IEquatable<ServerId>
#endif
    {
        // fields
        private readonly ClusterId _clusterId;
        private readonly EndPoint _endPoint;
        private readonly int _hashCode;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ServerId"/> class.
        /// </summary>
        /// <param name="clusterId">The cluster identifier.</param>
        /// <param name="endPoint">The end point.</param>
        public ServerId(ClusterId clusterId, EndPoint endPoint)
        {
            _clusterId = Ensure.IsNotNull(clusterId, nameof(clusterId));
            _endPoint = Ensure.IsNotNull(endPoint, nameof(endPoint));
            _hashCode = new Hasher()
                .Hash(_clusterId)
                .Hash(_endPoint)
                .GetHashCode();
        }

#if NET45
        private ServerId(SerializationInfo info, StreamingContext context)
        {
            _clusterId = (ClusterId)info.GetValue("_clusterId", typeof(ClusterId));
            _endPoint = EndPointHelper.FromObjectData((List<object>)info.GetValue("_endPoint", typeof(List<object>)));
            _hashCode = new Hasher()
                .Hash(_clusterId)
                .Hash(_endPoint)
                .GetHashCode();
        }
#endif

        // properties
        /// <summary>
        /// Gets the cluster identifier.
        /// </summary>
        /// <value>
        /// The cluster identifier.
        /// </value>
        public ClusterId ClusterId
        {
            get { return _clusterId; }
        }

        /// <summary>
        /// Gets the end point.
        /// </summary>
        /// <value>
        /// The end point.
        /// </value>
        public EndPoint EndPoint
        {
            get { return _endPoint; }
        }

        // methods
        /// <inheritdoc/>
        public bool Equals(ServerId other)
        {
            if (other == null)
            {
                return false;
            }
            return
                _clusterId.Equals(other._clusterId) &&
                EndPointHelper.Equals(_endPoint, other._endPoint);
        }

        /// <inheritdoc/>
        public override bool Equals(object obj)
        {
            return Equals(obj as ServerId);
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return string.Format("{{ ClusterId : {0}, EndPoint : \"{1}\" }}", _clusterId, _endPoint);
        }

        // explicit interface implementations
#if NET45
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("_clusterId", _clusterId);
            info.AddValue("_endPoint", EndPointHelper.GetObjectData(_endPoint));
        }
#endif
    }
}
