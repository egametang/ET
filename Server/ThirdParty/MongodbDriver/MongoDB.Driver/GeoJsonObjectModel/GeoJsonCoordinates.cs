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
using System.Collections.ObjectModel;
using System.Linq;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver.GeoJsonObjectModel.Serializers;
using MongoDB.Shared;

namespace MongoDB.Driver.GeoJsonObjectModel
{
    /// <summary>
    /// Represents a GeoJson position in some coordinate system (see subclasses).
    /// </summary>
    [BsonSerializer(typeof(GeoJsonCoordinatesSerializer))]
    public abstract class GeoJsonCoordinates : IEquatable<GeoJsonCoordinates>
    {
        // public properties
        /// <summary>
        /// Gets the coordinate values.
        /// </summary>
        public abstract ReadOnlyCollection<double> Values { get; }

        // public operators
        /// <summary>
        /// Determines whether two <see cref="GeoJsonCoordinates"/> instances are equal.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        ///   <c>true</c> if the left hand side is equal to the right hand side; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(GeoJsonCoordinates lhs, GeoJsonCoordinates rhs)
        {
            return object.Equals(lhs, rhs); // handles lhs == null correctly
        }

        /// <summary>
        /// Determines whether two <see cref="GeoJsonCoordinates"/> instances are not equal.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        ///   <c>true</c> if the left hand side is not equal to the right hand side; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(GeoJsonCoordinates lhs, GeoJsonCoordinates rhs)
        {
            return !(lhs == rhs);
        }

        // public methods
        /// <summary>
        /// Determines whether the specified <see cref="GeoJsonCoordinates" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="GeoJsonCoordinates" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="GeoJsonCoordinates" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(GeoJsonCoordinates obj)
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
            var rhs = (GeoJsonCoordinates)obj;
            return Values.SequenceEqual(rhs.Values);
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
                .Hash(GetType().GetHashCode())
                .HashStructElements(Values)
                .GetHashCode();
        }
    }
}
