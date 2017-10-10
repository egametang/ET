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
    /// Represents a JSON encoder for a KillCursors message.
    /// </summary>
    public class KillCursorsMessageJsonEncoder : MessageJsonEncoderBase, IMessageEncoder
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="KillCursorsMessageJsonEncoder"/> class.
        /// </summary>
        /// <param name="textReader">The text reader.</param>
        /// <param name="textWriter">The text writer.</param>
        /// <param name="encoderSettings">The encoder settings.</param>
        public KillCursorsMessageJsonEncoder(TextReader textReader, TextWriter textWriter, MessageEncoderSettings encoderSettings)
            : base(textReader, textWriter, encoderSettings)
        {
        }

        // methods
        /// <summary>
        /// Reads the message.
        /// </summary>
        /// <returns>A message.</returns>
        public KillCursorsMessage ReadMessage()
        {
            var jsonReader = CreateJsonReader();
            var messageContext = BsonDeserializationContext.CreateRoot(jsonReader);
            var messageDocument = BsonDocumentSerializer.Instance.Deserialize(messageContext);

            var opcode = messageDocument["opcode"].AsString;
            if (opcode != "killCursors")
            {
                throw new FormatException("Opcode is not killCursors.");
            }

            var requestId = messageDocument["requestId"].ToInt32();
            var cursorIds = messageDocument["cursorIds"].AsBsonArray.Select(v => v.ToInt64());

            return new KillCursorsMessage(
                requestId,
                cursorIds);
        }

        /// <summary>
        /// Writes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteMessage(KillCursorsMessage message)
        {
            Ensure.IsNotNull(message, nameof(message));

            var messageDocument = new BsonDocument
            {
                { "opcode", "killCursors" },
                { "requestId", message.RequestId },
                { "cursorIds", new BsonArray(message.CursorIds.Select(id => new BsonInt64(id))) }
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
            WriteMessage((KillCursorsMessage)message);
        }
    }
}
