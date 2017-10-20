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
using System.IO;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders
{
    /// <summary>
    /// Represents a binary encoder for a Query message.
    /// </summary>
    public class QueryMessageBinaryEncoder : MessageBinaryEncoderBase, IMessageEncoder
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryMessageBinaryEncoder"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="encoderSettings">The encoder settings.</param>
        public QueryMessageBinaryEncoder(Stream stream, MessageEncoderSettings encoderSettings)
            : base(stream, encoderSettings)
        {
        }

        // methods
        private QueryFlags BuildQueryFlags(QueryMessage message)
        {
            var flags = QueryFlags.None;
            if (message.NoCursorTimeout)
            {
                flags |= QueryFlags.NoCursorTimeout;
            }
            if (message.OplogReplay)
            {
                flags |= QueryFlags.OplogReplay;
            }
            if (message.PartialOk)
            {
                flags |= QueryFlags.Partial;
            }
            if (message.SlaveOk)
            {
                flags |= QueryFlags.SlaveOk;
            }
            if (message.TailableCursor)
            {
                flags |= QueryFlags.TailableCursor;
            }
            if (message.AwaitData)
            {
                flags |= QueryFlags.AwaitData;
            }
            return flags;
        }

        /// <summary>
        /// Reads the message.
        /// </summary>
        /// <returns>A message.</returns>
        public QueryMessage ReadMessage()
        {
            return ReadMessage<BsonDocument>(BsonDocumentSerializer.Instance);
        }

        internal QueryMessage ReadMessage<TDocument>(IBsonSerializer<TDocument> serializer)
            where TDocument : BsonDocument
        {
            var binaryReader = CreateBinaryReader();
            var stream = binaryReader.BsonStream;
            var startPosition = stream.Position;

            var messageSize = stream.ReadInt32();
            var requestId = stream.ReadInt32();
            stream.ReadInt32(); // responseTo
            stream.ReadInt32(); // opcode
            var flags = (QueryFlags)stream.ReadInt32();
            var fullCollectionName = stream.ReadCString(Encoding);
            var skip = stream.ReadInt32();
            var batchSize = stream.ReadInt32();
            var context = BsonDeserializationContext.CreateRoot(binaryReader);
            var query = serializer.Deserialize(context);
            BsonDocument fields = null;
            if (stream.Position < startPosition + messageSize)
            {
                fields = serializer.Deserialize(context);
            }

            var awaitData = (flags & QueryFlags.AwaitData) == QueryFlags.AwaitData;
            var slaveOk = (flags & QueryFlags.SlaveOk) == QueryFlags.SlaveOk;
            var partialOk = (flags & QueryFlags.Partial) == QueryFlags.Partial;
            var noCursorTimeout = (flags & QueryFlags.NoCursorTimeout) == QueryFlags.NoCursorTimeout;
            var oplogReplay = (flags & QueryFlags.OplogReplay) == QueryFlags.OplogReplay;
            var tailableCursor = (flags & QueryFlags.TailableCursor) == QueryFlags.TailableCursor;

            return new QueryMessage(
                requestId,
                CollectionNamespace.FromFullName(fullCollectionName),
                query,
                fields,
                NoOpElementNameValidator.Instance,
                skip,
                batchSize,
                slaveOk,
                partialOk,
                noCursorTimeout,
                oplogReplay,
                tailableCursor,
                awaitData);
        }

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteMessage(QueryMessage message)
        {
            Ensure.IsNotNull(message, nameof(message));

            var binaryWriter = CreateBinaryWriter();
            var stream = binaryWriter.BsonStream;
            var startPosition = stream.Position;

            stream.WriteInt32(0); // messageSize
            stream.WriteInt32(message.RequestId);
            stream.WriteInt32(0); // responseTo
            stream.WriteInt32((int)Opcode.Query);
            stream.WriteInt32((int)BuildQueryFlags(message));
            stream.WriteCString(message.CollectionNamespace.FullName);
            stream.WriteInt32(message.Skip);
            stream.WriteInt32(message.BatchSize);
            WriteQuery(binaryWriter, message.Query, message.QueryValidator);
            WriteOptionalFields(binaryWriter, message.Fields);
            stream.BackpatchSize(startPosition);
        }

        private void WriteOptionalFields(BsonBinaryWriter binaryWriter, BsonDocument fields)
        {
            if (fields != null)
            {
                var context = BsonSerializationContext.CreateRoot(binaryWriter);
                BsonDocumentSerializer.Instance.Serialize(context, fields);
            }
        }

        private void WriteQuery(BsonBinaryWriter binaryWriter, BsonDocument query, IElementNameValidator queryValidator)
        {
            binaryWriter.PushElementNameValidator(queryValidator);
            try
            {
                var context = BsonSerializationContext.CreateRoot(binaryWriter);
                BsonDocumentSerializer.Instance.Serialize(context, query ?? new BsonDocument());
            }
            finally
            {
                binaryWriter.PopElementNameValidator();
            }
        }

        // explicit interface implementations
        MongoDBMessage IMessageEncoder.ReadMessage()
        {
            return ReadMessage();
        }

        void IMessageEncoder.WriteMessage(MongoDBMessage message)
        {
            WriteMessage((QueryMessage)message);
        }

        // nested types
        [Flags]
        private enum QueryFlags
        {
            None = 0,
            TailableCursor = 2,
            SlaveOk = 4,
            OplogReplay = 8,
            NoCursorTimeout = 16,
            AwaitData = 32,
            Exhaust = 64,
            Partial = 128
        }
    }
}
