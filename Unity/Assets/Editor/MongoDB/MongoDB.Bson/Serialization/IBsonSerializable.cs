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
using MongoDB.Bson.IO;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// An interface implemented by classes that handle their own BSON serialization.
    /// </summary>
    public interface IBsonSerializable
    {
        /// <summary>
        /// Deserializes this object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>Normally itself, though sometimes an instance of a subclass or null.</returns>
        object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options);
        /// <summary>
        /// Gets the document Id.
        /// </summary>
        /// <param name="id">The Id.</param>
        /// <param name="idNominalType">The nominal type of the Id.</param>
        /// <param name="idGenerator">The IdGenerator for the Id type.</param>
        /// <returns>True if the document has an Id.</returns>
        bool GetDocumentId(out object id, out Type idNominalType, out IIdGenerator idGenerator);
        /// <summary>
        /// Serializes this object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type of this object.</param>
        /// <param name="options">The serialization options.</param>
        void Serialize(BsonWriter bsonWriter, Type nominalType, IBsonSerializationOptions options);
        /// <summary>
        /// Sets the document Id.
        /// </summary>
        /// <param name="id">The Id.</param>
        void SetDocumentId(object id);
    }
}
