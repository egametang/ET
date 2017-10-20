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
using MongoDB.Driver.Core.Operations.ElementNameValidators;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders
{
    /// <summary>
    /// Represents a binary encoder for an Update message.
    /// </summary>
    public class UpdateMessageBinaryEncoder : MessageBinaryEncoderBase, IMessageEncoder
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="UpdateMessageBinaryEncoder"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="encoderSettings">The encoder settings.</param>
        public UpdateMessageBinaryEncoder(Stream stream, MessageEncoderSettings encoderSettings)
            : base(stream, encoderSettings)
        {
        }

        // methods
        private UpdateFlags BuildUpdateFlags(UpdateMessage message)
        {
            var flags = UpdateFlags.None;
            if (message.IsMulti)
            {
                flags |= UpdateFlags.Multi;
            }
            if (message.IsUpsert)
            {
                flags |= UpdateFlags.Upsert;
            }
            return flags;
        }

        /// <summary>
        /// Reads the message.
        /// </summary>
        /// <returns>A message.</returns>
        public UpdateMessage ReadMessage()
        {
            return ReadMessage(BsonDocumentSerializer.Instance);
        }

        internal UpdateMessage ReadMessage<TDocument>(IBsonSerializer<TDocument> serializer)
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
            var flags = (UpdateFlags)stream.ReadInt32();
            var context = BsonDeserializationContext.CreateRoot(binaryReader);
            var query = serializer.Deserialize(context);
            var update = serializer.Deserialize(context);

            var isMulti = (flags & UpdateFlags.Multi) == UpdateFlags.Multi;
            var isUpsert = (flags & UpdateFlags.Upsert) == UpdateFlags.Upsert;

            return new UpdateMessage(
                requestId,
                CollectionNamespace.FromFullName(fullCollectionName),
                query,
                update,
                NoOpElementNameValidator.Instance,
                isMulti,
                isUpsert);
        }

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteMessage(UpdateMessage message)
        {
            Ensure.IsNotNull(message, nameof(message));

            var binaryWriter = CreateBinaryWriter();
            var stream = binaryWriter.BsonStream;
            var startPosition = stream.Position;

            stream.WriteInt32(0); // messageSize
            stream.WriteInt32(message.RequestId);
            stream.WriteInt32(0); // responseTo
            stream.WriteInt32((int)Opcode.Update);
            stream.WriteInt32(0); // reserved
            stream.WriteCString(message.CollectionNamespace.FullName);
            stream.WriteInt32((int)BuildUpdateFlags(message));
            WriteQuery(binaryWriter, message.Query);
            WriteUpdate(binaryWriter, message.Update, message.UpdateValidator);
            stream.BackpatchSize(startPosition);
        }

        private void WriteQuery(BsonBinaryWriter binaryWriter, BsonDocument query)
        {
            var context = BsonSerializationContext.CreateRoot(binaryWriter);
            BsonDocumentSerializer.Instance.Serialize(context, query);
        }

        private void WriteUpdate(BsonBinaryWriter binaryWriter, BsonDocument update, IElementNameValidator updateValidator)
        {
            binaryWriter.PushElementNameValidator(updateValidator);
            try
            {
                var position = binaryWriter.BaseStream.Position;
                var context = BsonSerializationContext.CreateRoot(binaryWriter);
                BsonDocumentSerializer.Instance.Serialize(context, update);
                if (updateValidator is UpdateElementNameValidator && binaryWriter.BaseStream.Position == position + 5)
                {
                    throw new BsonSerializationException("Update documents cannot be empty.");
                }
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
            WriteMessage((UpdateMessage)message);
        }

        // nested types
        [Flags]
        private enum UpdateFlags
        {
            None = 0,
            Upsert = 1,
            Multi = 2
        }
    }
}
