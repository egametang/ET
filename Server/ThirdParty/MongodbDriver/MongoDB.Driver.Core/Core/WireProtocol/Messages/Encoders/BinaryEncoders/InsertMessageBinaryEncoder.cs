/* Copyright 2013-present MongoDB Inc.
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
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Operations.ElementNameValidators;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders
{
    /// <summary>
    /// Represents a binary encoder for an Insert message.
    /// </summary>
    /// <typeparam name="TDocument">The type of the documents.</typeparam>
    public class InsertMessageBinaryEncoder<TDocument> : MessageBinaryEncoderBase, IMessageEncoder
    {
        // private fields
        private readonly IBsonSerializer<TDocument> _serializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="InsertMessageBinaryEncoder{TDocument}"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="encoderSettings">The encoder settings.</param>
        /// <param name="serializer">The serializer.</param>
        public InsertMessageBinaryEncoder(Stream stream, MessageEncoderSettings encoderSettings, IBsonSerializer<TDocument> serializer)
            : base(stream, encoderSettings)
        {
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
        }

        // public methods
        /// <summary>
        /// Reads the message.
        /// </summary>
        /// <returns>A message.</returns>
        public InsertMessage<TDocument> ReadMessage()
        {
            var reader = CreateBinaryReader();
            var stream = reader.BsonStream;
            var messageStartPosition = stream.Position;

            var messageSize = stream.ReadInt32();
            var requestId = stream.ReadInt32();
            stream.ReadInt32(); // responseTo
            stream.ReadInt32(); // opcode
            var flags = (InsertFlags)stream.ReadInt32();
            var fullCollectionName = stream.ReadCString(Encoding);
            var documents = ReadDocuments(reader, messageStartPosition, messageSize);

            var documentSource = new BatchableSource<TDocument>(documents, canBeSplit: false);
            var maxBatchCount = int.MaxValue;
            var maxMessageSize = int.MaxValue;
            var continueOnError = (flags & InsertFlags.ContinueOnError) == InsertFlags.ContinueOnError;

            return new InsertMessage<TDocument>(
                requestId,
                CollectionNamespace.FromFullName(fullCollectionName),
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

            var writer = CreateBinaryWriter();
            var stream = writer.BsonStream;
            var messageStartPosition = (int)stream.Position;

            stream.WriteInt32(0); // messageSize
            stream.WriteInt32(message.RequestId);
            stream.WriteInt32(0); // responseTo
            stream.WriteInt32((int)Opcode.Insert);
            stream.WriteInt32((int)BuildInsertFlags(message));
            stream.WriteCString(message.CollectionNamespace.FullName);
            WriteDocuments(writer, messageStartPosition, message);
            stream.BackpatchSize(messageStartPosition);
        }

        // private methods
        private InsertFlags BuildInsertFlags(InsertMessage<TDocument> message)
        {
            var flags = InsertFlags.None;
            if (message.ContinueOnError)
            {
                flags |= InsertFlags.ContinueOnError;
            }
            return flags;
        }

        private List<TDocument> ReadDocuments(BsonBinaryReader reader, long messageStartPosition, int messageSize)
        {
            var stream = reader.BsonStream;
            var context = BsonDeserializationContext.CreateRoot(reader);
            var documents = new List<TDocument>();

            while (stream.Position < messageStartPosition + messageSize)
            {
                var document = _serializer.Deserialize(context);
                documents.Add(document);
            }

            return documents;
        }

        private void WriteDocuments(BsonBinaryWriter writer, long messageStartPosition, InsertMessage<TDocument> message)
        {
            var stream = writer.BsonStream;
            var context = BsonSerializationContext.CreateRoot(writer);

            var collectionNamespace = message.CollectionNamespace;
            var isSystemIndexesCollection = collectionNamespace.Equals(collectionNamespace.DatabaseNamespace.SystemIndexesCollection);
            var elementNameValidator = isSystemIndexesCollection ? (IElementNameValidator)NoOpElementNameValidator.Instance : CollectionElementNameValidator.Instance;

            writer.PushElementNameValidator(elementNameValidator);
            try
            {
                var documentSource = message.DocumentSource;
                var batchCount = Math.Min(documentSource.Count, message.MaxBatchCount);
                if (batchCount < documentSource.Count && !documentSource.CanBeSplit)
                {
                    throw new BsonSerializationException("Batch is too large.");
                }

                for (var i = 0; i < batchCount; i++)
                {
                    var document = documentSource.Items[documentSource.Offset + i];
                    var documentStartPosition = stream.Position;

                    _serializer.Serialize(context, document);

                    var messageSize = stream.Position - messageStartPosition;
                    if (messageSize > message.MaxMessageSize)
                    {
                        if (i > 0 && documentSource.CanBeSplit)
                        {
                            stream.Position = documentStartPosition;
                            stream.SetLength(documentStartPosition);
                            documentSource.SetProcessedCount(i);
                            return;
                        }
                        else
                        {
                            throw new BsonSerializationException("Batch is too large.");
                        }
                    }
                }
                documentSource.SetProcessedCount(batchCount);
            }
            finally
            {
                writer.PopElementNameValidator();
            }
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

        // nested types
        [Flags]
        private enum InsertFlags
        {
            None = 0,
            ContinueOnError = 1
        }
    }
}
