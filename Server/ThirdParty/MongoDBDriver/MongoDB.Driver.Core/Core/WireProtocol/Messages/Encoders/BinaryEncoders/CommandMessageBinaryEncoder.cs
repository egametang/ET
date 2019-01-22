/* Copyright 2018-present MongoDB Inc.
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
* See the License for the specific language governing per
* ssions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders
{
    /// <summary>
    /// Represents a binary encoder for a CommandMessage.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders.MessageBinaryEncoderBase" />
    /// <seealso cref="MongoDB.Driver.Core.WireProtocol.Messages.Encoders.IMessageEncoder" />
    public class CommandMessageBinaryEncoder : MessageBinaryEncoderBase, IMessageEncoder
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandMessageBinaryEncoder" /> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="encoderSettings">The encoder settings.</param>
        public CommandMessageBinaryEncoder(Stream stream, MessageEncoderSettings encoderSettings)
            : base(stream, encoderSettings)
        {
        }

        // public methods
        /// <summary>
        /// Reads the message.
        /// </summary>
        /// <returns>A message.</returns>
        public CommandMessage ReadMessage()
        {
            var reader = CreateBinaryReader();
            var stream = reader.BsonStream;
            var messageStartPosition = stream.Position;

            var messageLength = stream.ReadInt32();
            EnsureMessageLengthIsValid(messageLength);
            var messageEndPosition = messageStartPosition + messageLength;

            var requestId = stream.ReadInt32();
            var responseTo = stream.ReadInt32();
            var opcode = (Opcode)stream.ReadInt32();
            EnsureOpcodeIsValid(opcode);
            var flags = (OpMsgFlags)stream.ReadInt32();
            EnsureFlagsAreValid(flags);
            var moreToCome = (flags & OpMsgFlags.MoreToCome) != 0;
            var sections = ReadSections(reader, messageEndPosition);
            EnsureExactlyOneType0SectionIsPresent(sections);
            EnsureMessageEndedAtEndPosition(stream, messageEndPosition);

            return new CommandMessage(requestId, responseTo, sections, moreToCome);
        }

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteMessage(CommandMessage message)
        {
            Ensure.IsNotNull(message, nameof(message));

            var writer = CreateBinaryWriter();
            var stream = writer.BsonStream;
            var messageStartPosition = stream.Position;

            stream.WriteInt32(0); // messageLength
            stream.WriteInt32(message.RequestId);
            stream.WriteInt32(message.ResponseTo);
            stream.WriteInt32((int)Opcode.OpMsg);
            stream.WriteInt32((int)CreateFlags(message));
            WriteSections(writer, message.Sections, messageStartPosition);
            stream.BackpatchSize(messageStartPosition);

            message.PostWriteAction?.Invoke(new PostProcessor(message, stream, messageStartPosition));
        }

        // explicit interface implementations
        MongoDBMessage IMessageEncoder.ReadMessage()
        {
            return ReadMessage();
        }

        void IMessageEncoder.WriteMessage(MongoDBMessage message)
        {
            WriteMessage((CommandMessage)message);
        }

        // private methods
        private OpMsgFlags CreateFlags(CommandMessage message)
        {
            var flags = (OpMsgFlags)0;
            if (message.MoreToCome)
            {
                flags |= OpMsgFlags.MoreToCome;
            }
            return flags;
        }

        private void EnsureExactlyOneType0SectionIsPresent(List<CommandMessageSection> sections)
        {
            var count = sections.Count(s => s.PayloadType == PayloadType.Type0);
            switch (count)
            {
                case 0:
                    throw new FormatException("Command message has no type 0 section.");

                case 1:
                    return;

                default:
                    throw new FormatException("Command message has more than one type 0 section.");
            }
        }

        private void EnsureFlagsAreValid(OpMsgFlags flags)
        {
            var invalidFlags = ~OpMsgFlags.All;
            if ((flags & invalidFlags) != 0)
            {
                throw new FormatException("Command message has invalid flags.");
            }
            if ((flags & OpMsgFlags.ChecksumPresent) != 0)
            {
                throw new FormatException("Command message CheckSumPresent flag not supported.");
            }
        }

        private void EnsureMessageEndedAtEndPosition(BsonStream stream, long messageEndPosition)
        {
            if (stream.Position != messageEndPosition)
            {
                throw new FormatException("Command message did not end at the expected end position.");
            }
        }

        private void EnsureMessageLengthIsValid(int messageLength)
        {
            if (messageLength < 0)
            {
                throw new FormatException("Command message length is negative.");
            }
        }

        private void EnsureOpcodeIsValid(Opcode opcode)
        {
            if (opcode != Opcode.OpMsg)
            {
                throw new FormatException("Command message opcode is not OP_MSG.");
            }
        }

        private void EnsurePayloadEndedAtEndPosition(BsonStream stream, long sectionEndPosition)
        {
            if (stream.Position != sectionEndPosition)
            {
                throw new FormatException("Command message payload did not end at the expected end position.");
            }
        }

        private void EnsureType1PayloadLengthIsValid(int sectionLength)
        {
            if (sectionLength < 0)
            {
                throw new FormatException("Command message type 1 payload length is negative.");
            }
        }

        private CommandMessageSection ReadSection(BsonBinaryReader reader)
        {
            var payloadType = reader.BsonStream.ReadByte();
            if (payloadType == -1)
            {
                throw new EndOfStreamException();
            }

            switch (payloadType)
            {
                case 0:
                    return ReadType0Section(reader);

                case 1:
                    return ReadType1Section(reader);

                default:
                    throw new FormatException($"Command message invalid payload type: {payloadType}.");
            }
        }

        private List<CommandMessageSection> ReadSections(BsonBinaryReader reader, long messageEndPosition)
        {
            var sections = new List<CommandMessageSection>();
            while (reader.BsonStream.Position < messageEndPosition)
            {
                var section = ReadSection(reader);
                sections.Add(section);
            }
            return sections;
        }

        private Type0CommandMessageSection<RawBsonDocument> ReadType0Section(IBsonReader reader)
        {
            var serializer = RawBsonDocumentSerializer.Instance;
            var context = BsonDeserializationContext.CreateRoot(reader);
            var document = serializer.Deserialize(context);
            return new Type0CommandMessageSection<RawBsonDocument>(document, serializer);
        }

        private Type1CommandMessageSection<RawBsonDocument> ReadType1Section(BsonBinaryReader reader)
        {
            var stream = reader.BsonStream;

            var payloadStartPosition = stream.Position;
            var payloadLength = stream.ReadInt32();
            EnsureType1PayloadLengthIsValid(payloadLength);
            var payloadEndPosition = payloadStartPosition + payloadLength;
            var identifier = stream.ReadCString(Utf8Encodings.Strict);
            var serializer = RawBsonDocumentSerializer.Instance;
            var context = BsonDeserializationContext.CreateRoot(reader);
            var documents = new List<RawBsonDocument>();
            while (stream.Position < payloadEndPosition)
            {
                var document = serializer.Deserialize(context);
                documents.Add(document);
            }
            EnsurePayloadEndedAtEndPosition(stream, payloadEndPosition);
            var batch = new BatchableSource<RawBsonDocument>(documents, canBeSplit: false);

            return new Type1CommandMessageSection<RawBsonDocument>(identifier, batch, serializer, NoOpElementNameValidator.Instance, null, null);
        }

        private void WriteSection(BsonBinaryWriter writer, CommandMessageSection section, long messageStartPosition)
        {
            writer.BsonStream.WriteByte((byte)section.PayloadType);

            switch (section.PayloadType)
            {
                case PayloadType.Type0:
                    WriteType0Section(writer, (Type0CommandMessageSection)section);
                    break;

                case PayloadType.Type1:
                    WriteType1Section(writer, (Type1CommandMessageSection)section, messageStartPosition);
                    break;

                default:
                    throw new ArgumentException("Invalid payload type.", nameof(section));
            }
        }

        private void WriteSections(BsonBinaryWriter writer, IEnumerable<CommandMessageSection> sections, long messageStartPosition)
        {
            foreach (var section in sections)
            {
                WriteSection(writer, section, messageStartPosition);
            }
        }

        private void WriteType0Section(BsonBinaryWriter writer, Type0CommandMessageSection section)
        {
            var serializer = section.DocumentSerializer;
            var context = BsonSerializationContext.CreateRoot(writer);
            serializer.Serialize(context, section.Document);
        }

        private void WriteType1Section(BsonBinaryWriter writer, Type1CommandMessageSection section, long messageStartPosition)
        {
            var stream = writer.BsonStream;
            var serializer = section.DocumentSerializer;
            var context = BsonSerializationContext.CreateRoot(writer);
            var maxMessageSize = MaxMessageSize;

            var payloadStartPosition = stream.Position;
            stream.WriteInt32(0); // size
            stream.WriteCString(section.Identifier);

            var batch = section.Documents;
            var maxDocumentSize = section.MaxDocumentSize ?? writer.Settings.MaxDocumentSize;
            writer.PushSettings(s => ((BsonBinaryWriterSettings)s).MaxDocumentSize = maxDocumentSize);
            writer.PushElementNameValidator(section.ElementNameValidator);
            try
            {
                var maxBatchCount = Math.Min(batch.Count, section.MaxBatchCount ?? int.MaxValue);
                var processedCount = maxBatchCount;
                for (var i = 0; i < maxBatchCount; i++)
                {
                    var documentStartPosition = stream.Position;
                    var document = batch.Items[batch.Offset + i];
                    serializer.Serialize(context, document);

                    var messageSize = stream.Position - messageStartPosition;
                    if (messageSize > maxMessageSize && batch.CanBeSplit)
                    {
                        stream.Position = documentStartPosition;
                        stream.SetLength(documentStartPosition);
                        processedCount = i;
                        break;
                    }
                }
                batch.SetProcessedCount(processedCount);
            }
            finally
            {
                writer.PopElementNameValidator();
                writer.PopSettings();
            }
            stream.BackpatchSize(payloadStartPosition);
        }

        // nested types
        private class PostProcessor : IMessageEncoderPostProcessor
        {
            // private fields
            private readonly CommandMessage _message;
            private readonly long _messageStartPosition;
            private readonly BsonStream _stream;

            // constructors
            public PostProcessor(CommandMessage message, BsonStream stream, long messageStartPosition)
            {
                _message = message;
                _stream = stream;
                _messageStartPosition = messageStartPosition;
            }

            // public methods
            public void ChangeWriteConcernFromW0ToW1()
            {
                ChangeMoreToComeFromTrueToFalse();
                ChangeWFrom0To1();
                _message.MoreToCome = false;
            }

            // private methods
            private void ChangeMoreToComeFromTrueToFalse()
            {
                var flagsPosition = _messageStartPosition + 16;
                _stream.Position = flagsPosition;
                var flags = (OpMsgFlags)_stream.ReadInt32();
                if ((flags & OpMsgFlags.MoreToCome) == 0)
                {
                    throw new InvalidOperationException("MoreToCome was not true.");
                }
                flags = flags & ~OpMsgFlags.MoreToCome;
                _stream.Position = flagsPosition;
                _stream.WriteInt32((int)flags);
            }

            private void ChangeWFrom0To1()
            {
                var wPosition = FindWPosition();
                _stream.Position = wPosition;
                var w = _stream.ReadInt32();
                if (w != 0)
                {
                    throw new InvalidOperationException("w was not 0.");
                }
                _stream.Position = wPosition;
                _stream.WriteInt32(1);
            }

            private long FindType0SectionPosition()
            {
                _stream.Position = _messageStartPosition;
                var messageLength = _stream.ReadInt32();

                _stream.Position = _messageStartPosition + 20;
                var messageEndPosition = _messageStartPosition + messageLength;
                while (_stream.Position < messageEndPosition)
                {
                    var sectionPosition = _stream.Position;

                    var payloadType = (PayloadType)_stream.ReadByte();
                    if (payloadType == PayloadType.Type0)
                    {
                        return sectionPosition;
                    }
                    else if (payloadType != PayloadType.Type1)
                    {
                        throw new FormatException($"Invalid payload type: {payloadType}.");
                    }

                    var sectionSize = _stream.ReadInt32();
                    _stream.Position = sectionPosition + 1 + sectionSize;
                }

                throw new InvalidOperationException("No type 0 section found.");
            }

            private long FindWPosition()
            {
                var type0SectionPosition = FindType0SectionPosition();

                _stream.Position = type0SectionPosition + 1;
                using (var reader = new BsonBinaryReader(_stream))
                {
                    reader.ReadStartDocument();
                    while (reader.ReadBsonType() != 0)
                    {
                        if (reader.ReadName() == "writeConcern")
                        {
                            reader.ReadStartDocument();
                            while (reader.ReadBsonType() != 0)
                            {
                                if (reader.ReadName() == "w")
                                {
                                    if (reader.CurrentBsonType == BsonType.Int32)
                                    {
                                        return _stream.Position;
                                    }
                                    goto notFound;
                                }

                                reader.SkipValue();
                            }
                            goto notFound;
                        }

                        reader.SkipValue();
                    }
                }

                notFound:
                throw new InvalidOperationException("{ w : <Int32> } not found.");
            }

        }
    }
}
