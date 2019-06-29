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
* See the License for the specific language governing permissions and
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

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders.JsonEncoders
{
    /// <summary>
    /// Represents a JSON encoder for a CommandMessage.
    /// </summary>
    /// <seealso cref="MongoDB.Driver.Core.WireProtocol.Messages.Encoders.JsonEncoders.MessageJsonEncoderBase" />
    /// <seealso cref="MongoDB.Driver.Core.WireProtocol.Messages.Encoders.IMessageEncoder" />
    public class CommandMessageJsonEncoder : MessageJsonEncoderBase, IMessageEncoder
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CommandMessageJsonEncoder"/> class.
        /// </summary>
        /// <param name="textReader">The text reader.</param>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="encoderSettings">The encoder settings.</param>
        public CommandMessageJsonEncoder(TextReader textReader, TextWriter textWriter, MessageEncoderSettings encoderSettings)
            : base(textReader, textWriter, encoderSettings)
        {
        }

        // public methods
        /// <summary>
        /// Reads the message.
        /// </summary>
        /// <returns>A message.</returns>
        public CommandMessage ReadMessage()
        {
            var reader = CreateJsonReader();
            var context = BsonDeserializationContext.CreateRoot(reader);
            var messageDocument = BsonDocumentSerializer.Instance.Deserialize(context);

            var opcode = messageDocument["opcode"].AsString;
            if (opcode != "opmsg")
            {
                throw new FormatException($"Command message invalid opcode: \"{opcode}\".");
            }
            var requestId = messageDocument["requestId"].ToInt32();
            var responseTo = messageDocument["responseTo"].ToInt32();
            var moreToCome = messageDocument.GetValue("moreToCome", false).ToBoolean();
            var sections = ReadSections(messageDocument["sections"].AsBsonArray.Cast<BsonDocument>());

            return new CommandMessage(requestId, responseTo, sections, moreToCome);
        }

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteMessage(CommandMessage message)
        {
            Ensure.IsNotNull(message, nameof(message));

            var writer = CreateJsonWriter();

            writer.WriteStartDocument();
            writer.WriteString("opcode", "opmsg");
            writer.WriteInt32("requestId", message.RequestId);
            writer.WriteInt32("responseTo", message.ResponseTo);
            if (message.MoreToCome)
            {
                writer.WriteBoolean("moreToCome", true);
            }
            writer.WriteName("sections");
            WriteSections(writer, message.Sections);
            writer.WriteEndDocument();
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
        private CommandMessageSection ReadSection(BsonDocument sectionDocument)
        {
            var payloadType = sectionDocument["payloadType"].ToInt32();
            switch (payloadType)
            {
                case 0:
                    return ReadType0Section(sectionDocument);

                case 1:
                    return ReadType1Section(sectionDocument);

                default:
                    throw new FormatException($"Command message invalid payload type: {payloadType}.");
            }
        }

        private IEnumerable<CommandMessageSection> ReadSections(IEnumerable<BsonDocument> sectionDocuments)
        {
            var sections = new List<CommandMessageSection>();
            foreach (var sectionDocument in sectionDocuments)
            {
                var section = ReadSection(sectionDocument);
                sections.Add(section);
            }
            return sections;
        }

        private CommandMessageSection ReadType0Section(BsonDocument sectionDocument)
        {
            var document = sectionDocument["document"].AsBsonDocument;
            return new Type0CommandMessageSection<BsonDocument>(document, BsonDocumentSerializer.Instance);
        }

        private CommandMessageSection ReadType1Section(BsonDocument sectionDocument)
        {
            var identifier = sectionDocument["identifier"].AsString;
            var documents = sectionDocument["documents"].AsBsonArray.Cast<BsonDocument>().ToList();
            var batch = new BatchableSource<BsonDocument>(documents, canBeSplit: false);
            return new Type1CommandMessageSection<BsonDocument>(identifier, batch, BsonDocumentSerializer.Instance, NoOpElementNameValidator.Instance, null, null);
        }

        private void WriteSection(IBsonWriter writer, CommandMessageSection section)
        {
            writer.WriteStartDocument();
            writer.WriteInt32("payloadType", (int)section.PayloadType);

            switch (section.PayloadType)
            {
                case PayloadType.Type0:
                    WriteType0Section(writer, (Type0CommandMessageSection)section);
                    break;

                case PayloadType.Type1:
                    WriteType1Section(writer, (Type1CommandMessageSection)section);
                    break;

                default:
                    throw new ArgumentException($"Invalid payload type: {section.PayloadType}.");
            }

            writer.WriteEndDocument();
        }

        private void WriteSections(IBsonWriter writer, IEnumerable<CommandMessageSection> sections)
        {
            writer.WriteStartArray();
            foreach (var section in sections)
            {
                WriteSection(writer, section);
            }
            writer.WriteEndArray();
        }

        private void WriteType0Section(IBsonWriter writer, Type0CommandMessageSection section)
        {
            writer.WriteName("document");
            var serializer = section.DocumentSerializer;
            var context = BsonSerializationContext.CreateRoot(writer);
            serializer.Serialize(context, section.Document);
        }

        private void WriteType1Section(IBsonWriter writer, Type1CommandMessageSection section)
        {
            writer.WriteString("identifier", section.Identifier);
            writer.WriteName("documents");
            writer.WriteStartArray();
            var batch = section.Documents;
            var serializer = section.DocumentSerializer;
            var context = BsonSerializationContext.CreateRoot(writer);
            for (var i = 0; i < batch.Count; i++)
            {
                var document = batch.Items[batch.Offset + i];
                serializer.Serialize(context, document);
            }
            writer.WriteEndArray();
        }
    }
}
