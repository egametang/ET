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
using System.Text;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders
{
    /// <summary>
    /// Represents a binary encoder for a Delete message.
    /// </summary>
    public class DeleteMessageBinaryEncoder : MessageBinaryEncoderBase, IMessageEncoder
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteMessageBinaryEncoder"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="encoderSettings">The encoder settings.</param>
        public DeleteMessageBinaryEncoder(Stream stream, MessageEncoderSettings encoderSettings)
            : base(stream, encoderSettings)
        {
        }

        // methods
        private DeleteFlags BuildDeleteFlags(DeleteMessage message)
        {
            var flags = DeleteFlags.None;
            if (!message.IsMulti)
            {
                flags |= DeleteFlags.Single;
            }
            return flags;
        }

        /// <summary>
        /// Reads the message.
        /// </summary>
        /// <returns>A message.</returns>
        public DeleteMessage ReadMessage()
        {
            return ReadMessage(BsonDocumentSerializer.Instance);
        }

        internal DeleteMessage ReadMessage<TDocument>(IBsonSerializer<TDocument> serializer)
            where TDocument : BsonDocument
        {
            var binaryReader = CreateBinaryReader();
            var stream = binaryReader.BsonStream;

            stream.ReadInt32(); // messageSize
            var requestId = stream.ReadInt32();
            stream.ReadInt32(); // responseTo
            stream.ReadInt32(); // opcode
            stream.ReadInt32(); // reserved
            var fullCollectionName = stream.ReadCString(Encoding);
            var flags = (DeleteFlags)stream.ReadInt32();
            var context = BsonDeserializationContext.CreateRoot(binaryReader);
            var query = serializer.Deserialize(context);

            var isMulti = (flags & DeleteFlags.Single) != DeleteFlags.Single;

            return new DeleteMessage(
                requestId,
                CollectionNamespace.FromFullName(fullCollectionName),
                query,
                isMulti);
        }

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteMessage(DeleteMessage message)
        {
            Ensure.IsNotNull(message, nameof(message));

            var binaryWriter = CreateBinaryWriter();
            var stream = binaryWriter.BsonStream;
            var startPosition = stream.Position;

            stream.WriteInt32(0); // messageSize
            stream.WriteInt32(message.RequestId);
            stream.WriteInt32(0); // responseTo
            stream.WriteInt32((int)Opcode.Delete);
            stream.WriteInt32(0); // reserved
            stream.WriteCString(message.CollectionNamespace.FullName);
            stream.WriteInt32((int)BuildDeleteFlags(message));
            var context = BsonSerializationContext.CreateRoot(binaryWriter);
            BsonDocumentSerializer.Instance.Serialize(context, message.Query ?? new BsonDocument());
            stream.BackpatchSize(startPosition);
        }

        // explicit interface implementations
        MongoDBMessage IMessageEncoder.ReadMessage()
        {
            return ReadMessage();
        }

        void IMessageEncoder.WriteMessage(MongoDBMessage message)
        {
            WriteMessage((DeleteMessage)message);
        }

        // nested types
        [Flags]
        private enum DeleteFlags
        {
            None = 0,
            Single = 1
        }
    }
}
