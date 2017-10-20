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
        // fields
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

        // methods
        private void AddDocument(State state, byte[] serializedDocument)
        {
            var binaryWriter = state.BinaryWriter;
            var stream = binaryWriter.BsonStream;
            stream.WriteBytes(serializedDocument, 0, serializedDocument.Length);
            state.BatchCount++;
            state.MessageSize = (int)stream.Position - state.MessageStartPosition;
        }

        private void AddDocument(State state, TDocument document)
        {
            var binaryWriter = state.BinaryWriter;

            var collectionNamespace = state.Message.CollectionNamespace;
            var isSystemIndexesCollection = collectionNamespace.Equals(collectionNamespace.DatabaseNamespace.SystemIndexesCollection);
            var elementNameValidator = isSystemIndexesCollection ? (IElementNameValidator)NoOpElementNameValidator.Instance : CollectionElementNameValidator.Instance;

            binaryWriter.PushElementNameValidator(elementNameValidator);
            try
            {
                var stream = binaryWriter.BsonStream;
                var context = BsonSerializationContext.CreateRoot(binaryWriter);
                _serializer.Serialize(context, document);
                state.BatchCount++;
                state.MessageSize = (int)stream.Position - state.MessageStartPosition;
            }
            finally
            {
                binaryWriter.PopElementNameValidator();
            }
        }

        private InsertFlags BuildInsertFlags(InsertMessage<TDocument> message)
        {
            var flags = InsertFlags.None;
            if (message.ContinueOnError)
            {
                flags |= InsertFlags.ContinueOnError;
            }
            return flags;
        }

        /// <summary>
        /// Reads the message.
        /// </summary>
        /// <returns>A message.</returns>
        public InsertMessage<TDocument> ReadMessage()
        {
            var binaryReader = CreateBinaryReader();
            var stream = binaryReader.BsonStream;
            var startPosition = stream.Position;

            var messageSize = stream.ReadInt32();
            var requestId = stream.ReadInt32();
            stream.ReadInt32(); // responseTo
            stream.ReadInt32(); // opcode
            var flags = (InsertFlags)stream.ReadInt32();
            var fullCollectionName = stream.ReadCString(Encoding);
            var documents = new List<TDocument>();
            while (stream.Position < startPosition + messageSize)
            {
                var context = BsonDeserializationContext.CreateRoot(binaryReader);
                var document = _serializer.Deserialize(context);
                documents.Add(document);
            }

            var documentSource = new BatchableSource<TDocument>(documents);
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

        private byte[] RemoveLastDocument(State state, int documentStartPosition)
        {
            var binaryWriter = state.BinaryWriter;
            var stream = binaryWriter.BsonStream;

            var documentSize = (int)stream.Position - documentStartPosition;
            stream.Position = documentStartPosition;
            var serializedDocument = new byte[documentSize];
            stream.ReadBytes(serializedDocument, 0, documentSize);
            stream.Position = documentStartPosition;
            stream.SetLength(documentStartPosition);
            state.BatchCount--;
            state.MessageSize = (int)stream.Position - state.MessageStartPosition;

            return serializedDocument;
        }

        private void WriteDocuments(State state)
        {
            if (state.Message.DocumentSource.Batch == null)
            {
                WriteNextBatch(state);
            }
            else
            {
                WriteSingleBatch(state);
            }
        }

        private void WriteSingleBatch(State state)
        {
            var message = state.Message;

            foreach (var document in message.DocumentSource.Batch)
            {
                AddDocument(state, document);

                if ((state.BatchCount > message.MaxBatchCount || state.MessageSize > message.MaxMessageSize) && state.BatchCount > 1)
                {
                    throw new ArgumentException("The non-batchable documents do not fit in a single Insert message.");
                }
            }
        }

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteMessage(InsertMessage<TDocument> message)
        {
            Ensure.IsNotNull(message, nameof(message));

            var binaryWriter = CreateBinaryWriter();
            var stream = binaryWriter.BsonStream;
            var messageStartPosition = (int)stream.Position;
            var state = new State { BinaryWriter = binaryWriter, Message = message, MessageStartPosition = messageStartPosition };

            stream.WriteInt32(0); // messageSize
            stream.WriteInt32(message.RequestId);
            stream.WriteInt32(0); // responseTo
            stream.WriteInt32((int)Opcode.Insert);
            stream.WriteInt32((int)BuildInsertFlags(message));
            stream.WriteCString(message.CollectionNamespace.FullName);
            WriteDocuments(state);
            stream.BackpatchSize(messageStartPosition);
        }

        private void WriteNextBatch(State state)
        {
            var batch = new List<TDocument>();

            var message = state.Message;
            var documentSource = message.DocumentSource;

            var overflow = documentSource.StartBatch();
            if (overflow != null)
            {
                batch.Add(overflow.Item);
                AddDocument(state, (byte[])overflow.State);
            }

            // always go one document too far so that we can detect when the docuemntSource runs out of documents
            while (documentSource.MoveNext())
            {
                var document = documentSource.Current;
                if (document == null)
                {
                    throw new ArgumentException("Batch contains one or more null documents.");
                }

                var binaryWriter = state.BinaryWriter;
                var stream = binaryWriter.BsonStream;
                var documentStartPosition = (int)stream.Position;
                AddDocument(state, document);

                if ((state.BatchCount > message.MaxBatchCount || state.MessageSize > message.MaxMessageSize) && state.BatchCount > 1)
                {
                    var serializedDocument = RemoveLastDocument(state, documentStartPosition);
                    overflow = new BatchableSource<TDocument>.Overflow { Item = document, State = serializedDocument };
                    documentSource.EndBatch(batch, overflow);
                    return;
                }

                batch.Add(document);
            }

            documentSource.EndBatch(batch);
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

        private class State
        {
            public BsonBinaryWriter BinaryWriter;
            public InsertMessage<TDocument> Message;
            public int MessageStartPosition;
            public int BatchCount;
            public int MessageSize;
        }
    }
}
