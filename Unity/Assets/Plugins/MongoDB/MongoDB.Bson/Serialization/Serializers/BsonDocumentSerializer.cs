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
using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Serializers
{
    /// <summary>
    /// Represents a serializer for BsonDocuments.
    /// </summary>
    public class BsonDocumentSerializer : BsonBaseSerializer, IBsonDocumentSerializer, IBsonIdProvider
    {
        // private static fields
        private static BsonDocumentSerializer __instance = new BsonDocumentSerializer();

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDocumentSerializer class.
        /// </summary>
        public BsonDocumentSerializer()
            : base(new DocumentSerializationOptions())
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
        /// Deserializes an object from a BsonReader.
        /// </summary>
        /// <param name="bsonReader">The BsonReader.</param>
        /// <param name="nominalType">The nominal type of the object.</param>
        /// <param name="actualType">The actual type of the object.</param>
        /// <param name="options">The serialization options.</param>
        /// <returns>An object.</returns>
        public override object Deserialize(
            BsonReader bsonReader,
            Type nominalType,
            Type actualType,
            IBsonSerializationOptions options)
        {
            VerifyTypes(nominalType, actualType, typeof(BsonDocument));

            var bsonType = bsonReader.GetCurrentBsonType();
            string message;
            switch (bsonType)
            {
                case BsonType.Document:
                    var documentSerializationOptions = (options ?? DocumentSerializationOptions.Defaults) as DocumentSerializationOptions;
                    if (documentSerializationOptions == null)
                    {
                        message = string.Format(
                            "Serialize method of BsonDocument expected serialization options of type {0}, not {1}.",
                            BsonUtils.GetFriendlyTypeName(typeof(DocumentSerializationOptions)),
                            BsonUtils.GetFriendlyTypeName(options.GetType()));
                        throw new BsonSerializationException(message);
                    }

                    bsonReader.ReadStartDocument();
                    var document = new BsonDocument(documentSerializationOptions.AllowDuplicateNames);
                    while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
                    {
                        var name = bsonReader.ReadName();
                        var value = (BsonValue)BsonValueSerializer.Instance.Deserialize(bsonReader, typeof(BsonValue), null);
                        document.Add(name, value);
                    }
                    bsonReader.ReadEndDocument();

                    return document;
                default:
                    message = string.Format("Cannot deserialize BsonDocument from BsonType {0}.", bsonType);
                    throw new Exception(message);
            }
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

            BsonElement idElement;
            if (bsonDocument.TryGetElement("_id", out idElement))
            {
                id = idElement.Value;
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
        /// Gets the serialization info for a member.
        /// </summary>
        /// <param name="memberName">The member name.</param>
        /// <returns>
        /// The serialization info for the member.
        /// </returns>
        public BsonSerializationInfo GetMemberSerializationInfo(string memberName)
        {
            return new BsonSerializationInfo(
                memberName,
                BsonValueSerializer.Instance,
                typeof(BsonValue),
                BsonValueSerializer.Instance.GetDefaultSerializationOptions());
        }

        /// <summary>
        /// Serializes an object to a BsonWriter.
        /// </summary>
        /// <param name="bsonWriter">The BsonWriter.</param>
        /// <param name="nominalType">The nominal type.</param>
        /// <param name="value">The object.</param>
        /// <param name="options">The serialization options.</param>
        public override void Serialize(
            BsonWriter bsonWriter,
            Type nominalType,
            object value,
            IBsonSerializationOptions options)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }

            // could get here with a BsonDocumentWrapper from BsonValueSerializer switch statement
            var wrapper = value as BsonDocumentWrapper;
            if (wrapper != null)
            {
                BsonDocumentWrapperSerializer.Instance.Serialize(bsonWriter, nominalType, value, null);
                return;
            }

            var rawBsonDocument = value as RawBsonDocument;
            if (rawBsonDocument != null)
            {
                RawBsonDocumentSerializer.Instance.Serialize(bsonWriter, nominalType, value, options);
                return;
            }

            var bsonDocument = (BsonDocument)value;
            var documentSerializationOptions = (options ?? DocumentSerializationOptions.Defaults) as DocumentSerializationOptions;
            if (documentSerializationOptions == null)
            {
                var message = string.Format(
                    "Serialize method of BsonDocument expected serialization options of type {0}, not {1}.",
                    BsonUtils.GetFriendlyTypeName(typeof(DocumentSerializationOptions)),
                    BsonUtils.GetFriendlyTypeName(options.GetType()));
                throw new BsonSerializationException(message);
            }

            bsonWriter.WriteStartDocument();
            BsonElement idElement = null;
            if (documentSerializationOptions.SerializeIdFirst && bsonDocument.TryGetElement("_id", out idElement))
            {
                bsonWriter.WriteName(idElement.Name);
                BsonValueSerializer.Instance.Serialize(bsonWriter, typeof(BsonValue), idElement.Value, null);
            }

            foreach (var element in bsonDocument)
            {
                // if serializeIdFirst is false then idElement will be null and no elements will be skipped
                if (!object.ReferenceEquals(element, idElement))
                {
                    bsonWriter.WriteName(element.Name);
                    BsonValueSerializer.Instance.Serialize(bsonWriter, typeof(BsonValue), element.Value, null);
                }
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

            BsonElement idElement;
            if (bsonDocument.TryGetElement("_id", out idElement))
            {
                idElement.Value = idBsonValue;
            }
            else
            {
                bsonDocument.InsertAt(0, new BsonElement("_id", idBsonValue));
            }
        }
    }
}
