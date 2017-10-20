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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders.JsonEncoders
{
    /// <summary>
    /// Represents a JSON encoder for a Reply message.
    /// </summary>
    /// <typeparam name="TDocument">The type of the documents.</typeparam>
    public class ReplyMessageJsonEncoder<TDocument> : MessageJsonEncoderBase, IMessageEncoder
    {
        // fields
        private readonly IBsonSerializer<TDocument> _serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplyMessageJsonEncoder{TDocument}"/> class.
        /// </summary>
        /// <param name="textReader">The text reader.</param>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="encoderSettings">The encoder settings.</param>
        /// <param name="serializer">The serializer.</param>
        public ReplyMessageJsonEncoder(TextReader textReader, TextWriter textWriter, MessageEncoderSettings encoderSettings, IBsonSerializer<TDocument> serializer)
            : base(textReader, textWriter, encoderSettings)
        {
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
        }

        // methods
        /// <summary>
        /// Reads the message.
        /// </summary>
        /// <returns>A message.</returns>
        public ReplyMessage<TDocument> ReadMessage()
        {
            var jsonReader = CreateJsonReader();
            var messageContext = BsonDeserializationContext.CreateRoot(jsonReader);
            var messageDocument = BsonDocumentSerializer.Instance.Deserialize(messageContext);

            var opcode = messageDocument["opcode"].AsString;
            if (opcode != "reply")
            {
                throw new FormatException("Opcode is not reply.");
            }

            var awaitCapable = messageDocument.GetValue("awaitCapable", false).ToBoolean();
            var cursorId = messageDocument["cursorId"].ToInt64();
            var cursorNotFound = messageDocument.GetValue("cursorNotFound", false).ToBoolean();
            var numberReturned = messageDocument["numberReturned"].ToInt32();
            var queryFailure = false;
            var requestId = messageDocument["requestId"].ToInt32();
            var responseTo = messageDocument["responseTo"].ToInt32();
            var startingFrom = messageDocument.GetValue("startingFrom", 0).ToInt32();

            List<TDocument> documents = null;
            if (messageDocument.Contains("documents"))
            {
                documents = new List<TDocument>();
                foreach (BsonDocument serializedDocument in messageDocument["documents"].AsBsonArray)
                {
                    using (var documentReader = new BsonDocumentReader(serializedDocument))
                    {
                        var documentContext = BsonDeserializationContext.CreateRoot(documentReader);
                        var document = _serializer.Deserialize(documentContext);
                        documents.Add(document);
                    }
                }
            }

            BsonDocument queryFailureDocument = null;
            if (messageDocument.Contains("queryFailure"))
            {
                queryFailure = true;
                queryFailureDocument = messageDocument["queryFailure"].AsBsonDocument;
            }

            return new ReplyMessage<TDocument>(
                awaitCapable,
                cursorId,
                cursorNotFound,
                documents,
                numberReturned,
                queryFailure,
                queryFailureDocument,
                requestId,
                responseTo,
                _serializer,
                startingFrom);
        }

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteMessage(ReplyMessage<TDocument> message)
        {
            Ensure.IsNotNull(message, nameof(message));

            BsonArray documents = null;
            if (!message.QueryFailure)
            {
                var wrappers = message.Documents.Select(d => new BsonDocumentWrapper(d, _serializer));
                documents = new BsonArray(wrappers);
            }

            var messageDocument = new BsonDocument
            {
                { "opcode", "reply" },
                { "requestId", message.RequestId },
                { "responseTo", message.ResponseTo },
                { "cursorId", message.CursorId },
                { "cursorNotFound", true, message.CursorNotFound },
                { "numberReturned", message.NumberReturned },
                { "startingFrom", message.StartingFrom, message.StartingFrom != 0 },
                { "awaitCapable", true, message.AwaitCapable },
                { "queryFailure", () => message.QueryFailureDocument, message.QueryFailure },
                { "documents", documents, documents != null }
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
            WriteMessage((ReplyMessage<TDocument>)message);
        }
    }
}
