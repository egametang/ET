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
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders.JsonEncoders
{
    /// <summary>
    /// Represents a JSON encoder for an Insert message.
    /// </summary>
    /// <typeparam name="TDocument">The type of the documents.</typeparam>
    public class InsertMessageJsonEncoder<TDocument> : MessageJsonEncoderBase, IMessageEncoder
    {
        // fields
        private readonly IBsonSerializer<TDocument> _serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InsertMessageJsonEncoder{TDocument}"/> class.
        /// </summary>
        /// <param name="textReader">The text reader.</param>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="encoderSettings">The encoder settings.</param>
        /// <param name="serializer">The serializer.</param>
        public InsertMessageJsonEncoder(TextReader textReader, TextWriter textWriter, MessageEncoderSettings encoderSettings, IBsonSerializer<TDocument> serializer)
            : base(textReader, textWriter, encoderSettings)
        {
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
        }

        // methods
        /// <summary>
        /// Reads the message.
        /// </summary>
        /// <returns>A message.</returns>
        public InsertMessage<TDocument> ReadMessage()
        {
            var jsonReader = CreateJsonReader();
            var messageContext = BsonDeserializationContext.CreateRoot(jsonReader);
            var messageDocument = BsonDocumentSerializer.Instance.Deserialize(messageContext);

            var opcode = messageDocument["opcode"].AsString;
            if (opcode != "insert")
            {
                throw new FormatException("Opcode is not insert.");
            }

            var requestId = messageDocument["requestId"].ToInt32();
            var databaseName = messageDocument["database"].AsString;
            var collectionName = messageDocument["collection"].AsString;
            var maxBatchCount = messageDocument["maxBatchCount"].ToInt32();
            var maxMessageSize = messageDocument["maxMessageSize"].ToInt32();
            var continueOnError = messageDocument["continueOnError"].ToBoolean();
            var documents = messageDocument["documents"];

            if (documents.IsBsonNull)
            {
                throw new FormatException("InsertMessageJsonEncoder requires documents to not be null.");
            }

            var batch = new List<TDocument>();
            foreach (BsonDocument serializedDocument in documents.AsBsonArray)
            {
                using (var documentReader = new BsonDocumentReader(serializedDocument))
                {
                    var documentContext = BsonDeserializationContext.CreateRoot(documentReader);
                    var document = _serializer.Deserialize(documentContext);
                    batch.Add(document);
                }
            }
            var documentSource = new BatchableSource<TDocument>(batch);

            return new InsertMessage<TDocument>(
                requestId,
                new CollectionNamespace(databaseName, collectionName),
                _serializer,
                documentSource,
                maxBatchCount,
                maxMessageSize,
                continueOnError);
        }

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteMessage(InsertMessage<TDocument> message)
        {
            Ensure.IsNotNull(message, nameof(message));

            BsonValue documents;
            if (message.DocumentSource.Batch == null)
            {
                documents = BsonNull.Value;
            }
            else
            {
                var array = new BsonArray();
                foreach (var document in message.DocumentSource.Batch)
                {
                    var wrappedDocument = new BsonDocumentWrapper(document, _serializer);
                    array.Add(wrappedDocument);
                }
                documents = array;
            }

            var messageDocument = new BsonDocument
            {
                { "opcode", "insert" },
                { "requestId", message.RequestId },
                { "database", message.CollectionNamespace.DatabaseNamespace.DatabaseName },
                { "collection", message.CollectionNamespace.CollectionName },
                { "maxBatchCount", message.MaxBatchCount },
                { "maxMessageSize", message.MaxMessageSize },
                { "continueOnError", message.ContinueOnError },
                { "documents", documents }
            };

            var jsonWriter = CreateJsonWriter();
            var messageContext = BsonSerializationContext.CreateRoot(jsonWriter);
            BsonDocumentSerializer.Instance.Serialize(messageContext, messageDocument);
        }

        // explicit interface implementations
        MongoDBMessage IMessageEncoder.ReadMessage()
        {
            return ReadMessage();
        }

        void IMessageEncoder.WriteMessage(MongoDBMessage message)
        {
            WriteMessage((InsertMessage<TDocument>)message);
        }
    }
}
