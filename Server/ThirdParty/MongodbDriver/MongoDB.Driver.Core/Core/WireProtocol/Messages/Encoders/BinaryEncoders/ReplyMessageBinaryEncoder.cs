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

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders
{
    /// <summary>
    /// Represents a binary encoder for a Reply message.
    /// </summary>
    /// <typeparam name="TDocument">The type of the documents.</typeparam>
    public class ReplyMessageBinaryEncoder<TDocument> : MessageBinaryEncoderBase, IMessageEncoder
    {
        // fields
        private readonly IBsonSerializer<TDocument> _serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplyMessageBinaryEncoder{TDocument}"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="encoderSettings">The encoder settings.</param>
        /// <param name="serializer">The serializer.</param>
        public ReplyMessageBinaryEncoder(Stream stream, MessageEncoderSettings encoderSettings, IBsonSerializer<TDocument> serializer)
            : base(stream, encoderSettings)
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
            var binaryReader = CreateBinaryReader();
            var stream = binaryReader.BsonStream;

            stream.ReadInt32(); // messageSize
            var requestId = stream.ReadInt32();
            var responseTo = stream.ReadInt32();
            stream.ReadInt32(); // opcode
            var flags = (ResponseFlags)stream.ReadInt32();
            var cursorId = stream.ReadInt64();
            var startingFrom = stream.ReadInt32();
            var numberReturned = stream.ReadInt32();
            List<TDocument> documents = null;
            BsonDocument queryFailureDocument = null;

            var awaitCapable = (flags & ResponseFlags.AwaitCapable) == ResponseFlags.AwaitCapable;
            var cursorNotFound = (flags & ResponseFlags.CursorNotFound) == ResponseFlags.CursorNotFound;
            var queryFailure = (flags & ResponseFlags.QueryFailure) == ResponseFlags.QueryFailure;

            if (queryFailure)
            {
                var context = BsonDeserializationContext.CreateRoot(binaryReader);
                queryFailureDocument = BsonDocumentSerializer.Instance.Deserialize(context);
            }
            else
            {
                documents = new List<TDocument>();
                for (var i = 0; i < numberReturned; i++)
                {
                    var allowDuplicateElementNames = typeof(TDocument) == typeof(BsonDocument);
                    var context = BsonDeserializationContext.CreateRoot(binaryReader, builder =>
                    {
                        builder.AllowDuplicateElementNames = allowDuplicateElementNames;
                    });
                    documents.Add(_serializer.Deserialize(context));
                }
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

            var binaryWriter = CreateBinaryWriter();
            var stream = binaryWriter.BsonStream;
            var startPosition = stream.Position;

            stream.WriteInt32(0); // messageSize
            stream.WriteInt32(message.RequestId);
            stream.WriteInt32(message.ResponseTo);
            stream.WriteInt32((int)Opcode.Reply);

            var flags = ResponseFlags.None;
            if (message.AwaitCapable)
            {
                flags |= ResponseFlags.AwaitCapable;
            }
            if (message.QueryFailure)
            {
                flags |= ResponseFlags.QueryFailure;
            }
            if (message.CursorNotFound)
            {
                flags |= ResponseFlags.CursorNotFound;
            }
            stream.WriteInt32((int)flags);

            stream.WriteInt64(message.CursorId);
            stream.WriteInt32(message.StartingFrom);
            stream.WriteInt32(message.NumberReturned);
            if (message.QueryFailure)
            {
                var context = BsonSerializationContext.CreateRoot(binaryWriter);
                _serializer.Serialize(context, message.QueryFailureDocument);
            }
            else
            {
                foreach (var doc in message.Documents)
                {
                    var context = BsonSerializationContext.CreateRoot(binaryWriter);
                    _serializer.Serialize(context, doc);
                }
            }
            stream.BackpatchSize(startPosition);
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

        // nested types
        [Flags]
        private enum ResponseFlags
        {
            None = 0,
            CursorNotFound = 1,
            QueryFailure = 2,
            AwaitCapable = 8
        }
    }
}
