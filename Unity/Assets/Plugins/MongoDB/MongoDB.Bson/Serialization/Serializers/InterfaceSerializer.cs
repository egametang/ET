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
using System.IO;
using System.Linq;
using System.Text;

using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for Interfaces.
    /// </summary>
    public class InterfaceSerializer : BsonBaseSerializer
    {
        // private static fields
        private static InterfaceSerializer __instance = new InterfaceSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the InterfaceSerializer class.
        /// </summary>
        public InterfaceSerializer()
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the InterfaceSerializer class.
        /// </summary>
        public static InterfaceSerializer Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Deserializes a document from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the document.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>A document.</returns>
        public override object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            IBsonSerializationOptions options)
        {
            if (!nominalType.IsInterface)
            {
                var message = string.Format("Nominal type must be an interface, not {0}.", nominalType.FullName);
                throw new ArgumentException(message, "nominalType");
            }

            var discriminatorConvention = BsonSerializer.LookupDiscriminatorConvention(nominalType);
            var actualType = discriminatorConvention.GetActualType(bsonReader, nominalType);
            return Deserialize(bsonReader, nominalType, actualType, options);
        }

        /// <summary>
        /// Deserializes a document from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the document.</param>
        /// <param name="actualType">The actual type of the document..</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>A document.</returns>
        public override object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            if (!nominalType.IsInterface)
            {
                var message = string.Format("Nominal type must be an interface, not {0}.", nominalType.FullName);
                throw new ArgumentException(message, "nominalType");
            }

            if (actualType == nominalType)
            {
                var message = string.Format("Unable to determine actual type of object to deserialize. NominalType is the interface {0}.", nominalType);
                throw new Exception(message);
            }

            var serializer = BsonSerializer.LookupSerializer(actualType);
            return serializer.Deserialize(bsonReader, nominalType, actualType, options);
        }

        /// <summary>
        /// Serializes a document to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The document.</param>
        /// <param name="options">The serialization options.</param>
        public override void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            if (!nominalType.IsInterface)
            {
                var message = string.Format("Nominal type must be an interface, not {0}.", nominalType.FullName);
                throw new ArgumentException(message, "nominalType");
            }

            if (value == null)
            {
                bsonWriter.WriteNull();
            }
            else
            {
                var actualType = value.GetType();
                var serializer = BsonSerializer.LookupSerializer(actualType);
                serializer.Serialize(bsonWriter, nominalType, value, options);
            }
        }
    }
}