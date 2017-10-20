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

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Represents a serializer that has a Representation property.
    /// </summary>
    public interface IRepresentationConfigurable
    {
        /// <summary>
        /// Gets the representation.
        /// </summary>
        /// <value>
        /// The representation.
        /// </value>
        BsonType Representation { get; }

        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified representation.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        IBsonSerializer WithRepresentation(BsonType representation);
    }

    /// <summary>
    /// Represents a serializer that has a Representation property.
    /// </summary>
    /// <typeparam name="TSerializer">The type of the serializer.</typeparam>
    public interface IRepresentationConfigurable<TSerializer> : IRepresentationConfigurable where TSerializer : IBsonSerializer
    {
        /// <summary>
        /// Returns a serializer that has been reconfigured with the specified representation.
        /// </summary>
        /// <param name="representation">The representation.</param>
        /// <returns>The reconfigured serializer.</returns>
        new TSerializer WithRepresentation(BsonType representation);
    }
}
