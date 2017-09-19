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
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.IdGenerators;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BsonDocuments.
    /// </summary>
    public class BsonDocumentSerializer : BsonValueSerializerBase<BsonDocument>, IBsonDocumentSerializer, IBsonIdProvider
    {
        // private static fields
        private static BsonDocumentSerializer __instance = new BsonDocumentSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDocumentSerializer class.
        /// </summary>
        public BsonDocumentSerializer()
            : base(BsonType.Document)
        {
        }

        // public static properties
        /// <summary>
        /// Gets an instance of the BsonDocumentSerializer class.
        /// </summary>
        public static BsonDocumentSerializer Instance
        {
            get { return __instance; }
        }

        // public methods
        /// <summary>
        /// Deserializes a value.
        /// </summary>
        /// <param name="context">The deserialization context.</param>
        /// <param name="args">The deserialization args.</param>
        /// <returns>A deserialized value.</returns>
        protected override BsonDocument DeserializeValue(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            var bsonReader = context.Reader;

            bsonReader.ReadStartDocument();
            var document = new BsonDocument(allowDuplicateNames: context.AllowDuplicateElementNames);
            while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
            {
                var name = bsonReader.ReadName();
                var value = BsonValueSerializer.Instance.Deserialize(context);
                document.Add(name, value);
            }
            bsonReader.ReadEndDocument();

            return document;
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
            var bsonDocument = (BsonDocument)document;

            BsonValue idBsonValue;
            if (bsonDocument.TryGetValue("_id", out idBsonValue))
            {
                id = idBsonValue;
                idGenerator = BsonSerializer.LookupIdGenerator(id.GetType());

                if (idGenerator == null)
                {
                    var idBinaryData = id as BsonBinaryData;
                    if (idBinaryData != null && (idBinaryData.SubType == BsonBinarySubType.UuidLegacy || idBinaryData.SubType == BsonBinarySubType.UuidStandard))
                    {
                        idGenerator = BsonBinaryDataGuidGenerator.GetInstance(idBinaryData.GuidRepresentation);
                    }
                }
            }
            else
            {
                id = null;
                idGenerator = BsonObjectIdGenerator.Instance;
            }

            idNominalType = typeof(BsonValue);
            return true;
        }

        /// <summary>
        /// Tries to get the serialization info for a member.
        /// </summary>
        /// <param name="memberName">Name of the member.</param>
        /// <param name="serializationInfo">The serialization information.</param>
        /// <returns>
        ///   <c>true</c> if the serialization info exists; otherwise <c>false</c>.
        /// </returns>
        public bool TryGetMemberSerializationInfo(string memberName, out BsonSerializationInfo serializationInfo)
        {
            serializationInfo = new BsonSerializationInfo(
                memberName,
                BsonValueSerializer.Instance,
                typeof(BsonValue));
            return true;
        }

        /// <summary>
        /// Serializes a value.
        /// </summary>
        /// <param name="context">The serialization context.</param>
        /// <param name="args">The serialization args.</param>
        /// <param name="value">The object.</param>
        protected override void SerializeValue(BsonSerializationContext context, BsonSerializationArgs args, BsonDocument value)
        {
            var bsonWriter = context.Writer;
            bsonWriter.WriteStartDocument();

            var alreadySerializedIndex = -1;
            if (args.SerializeIdFirst)
            {
                var idIndex = value.IndexOfName("_id");
                if (idIndex != -1)
                {
                    bsonWriter.WriteName("_id");
                    BsonValueSerializer.Instance.Serialize(context, value[idIndex]);
                    alreadySerializedIndex = idIndex;
                }
            }

            var elementCount = value.ElementCount;
            for (var index = 0; index < elementCount; index++)
            {
                if (index == alreadySerializedIndex)
                {
                    continue;
                }

                var element = value.GetElement(index);
                bsonWriter.WriteName(element.Name);
                BsonValueSerializer.Instance.Serialize(context, element.Value);
            }

            bsonWriter.WriteEndDocument();
        }

        /// <summary>
        /// Sets the document Id.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="id">The Id.</param>
        public void SetDocumentId(object document, object id)
        {
            if (document == null)
            {
                throw new ArgumentNullException("document");
            }
            if (id == null)
            {
                throw new ArgumentNullException("id");
            }

            var bsonDocument = (BsonDocument)document;
            var idBsonValue = id as BsonValue;
            if (idBsonValue == null)
            {
                idBsonValue = BsonValue.Create(id); // be helpful and provide automatic conversion to BsonValue if necessary
            }

            var idIndex = bsonDocument.IndexOfName("_id");
            if (idIndex != -1)
            {
                bsonDocument[idIndex] = idBsonValue;
            }
            else
            {
                bsonDocument.InsertAt(0, new BsonElement("_id", idBsonValue));
            }
        }
    }
}
