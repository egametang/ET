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

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for classes that implement IBsonSerializable.
    /// </summary>
    public class BsonIBsonSerializableSerializer : IBsonSerializer, IBsonIdProvider
    {
        // private static fields
        private static BsonIBsonSerializableSerializer __instance = new BsonIBsonSerializableSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonIBsonSerializableSerializer class.
        /// </summary>
        public BsonIBsonSerializableSerializer()
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the BsonIBsonSerializableSerializer class.
        /// </summary>
        public static BsonIBsonSerializableSerializer Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public object Deserialize(BsonReader bsonReader, Type nominalType, IBsonSerializationOptions options)
        {
            var value = (IBsonSerializable)Activator.CreateInstance(nominalType, true); // private default constructor OK
            return value.Deserialize(bsonReader, nominalType, options);
        }

        /// <summary>
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            var value = (IBsonSerializable)Activator.CreateInstance(actualType, true); // private default constructor OK
            return value.Deserialize(bsonReader, nominalType, options);
        }

        /// <summary>
        /// Gets the default serialization options for this serializer.
        /// </summary>
        /// <returns>The default serialization options for this serializer.</returns>
        public IBsonSerializationOptions GetDefaultSerializationOptions()
        {
            return null;
        }

        /// <summary>
        /// Gets the document Id.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="id">The Id.</param>
        /// <param name="idNominalType">The nominal type of the Id.</param>
        /// <param name="idGenerator">The IdGenerator for the Id type.</param>
        /// <returns>True if the document has an Id.</returns>
        public bool GetDocumentId(
            object document,
            out object id,
            out Type idNominalType,
            out IIdGenerator idGenerator)
        {
            var bsonSerializable = (IBsonSerializable)document;
            return bsonSerializable.GetDocumentId(out id, out idNominalType, out idGenerator);
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var serializable = (IBsonSerializable)value;
                serializable.Serialize(bsonWriter, nominalType, options);
            }
        }

        /// <summary>
        /// Sets the document Id.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="id">The Id.</param>
        public void SetDocumentId(object document, object id)
        {
            var bsonSerializable = (IBsonSerializable)document;
            bsonSerializable.SetDocumentId(id);
        }
    }
}
