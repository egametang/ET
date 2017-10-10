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
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders
{
    /// <summary>
    /// Represents a binary encoder for a KillCursors message.
    /// </summary>
    public class KillCursorsMessageBinaryEncoder : MessageBinaryEncoderBase, IMessageEncoder
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="KillCursorsMessageBinaryEncoder"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="encoderSettings">The encoder settings.</param>
        public KillCursorsMessageBinaryEncoder(Stream stream, MessageEncoderSettings encoderSettings)
            : base(stream, encoderSettings)
        {
        }

        // methods
        /// <summary>
        /// Reads the message.
        /// </summary>
        /// <returns>A message.</returns>
        public KillCursorsMessage ReadMessage()
        {
            var binaryReader = CreateBinaryReader();
            var stream = binaryReader.BsonStream;

            stream.ReadInt32(); // messageSize
            var requestId = stream.ReadInt32();
            stream.ReadInt32(); // responseTo
            stream.ReadInt32(); // opcode
            stream.ReadInt32(); // reserved
            var count = stream.ReadInt32();
            var cursorIds = new long[count];
            for (var i = 0; i < count; i++)
            {
                cursorIds[i] = stream.ReadInt64();
            }

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

            var binaryWriter = CreateBinaryWriter();
            var stream = binaryWriter.BsonStream;
            var startPosition = stream.Position;

            stream.WriteInt32(0); // messageSize
            stream.WriteInt32(message.RequestId);
            stream.WriteInt32(0); // responseTo
            stream.WriteInt32((int)Opcode.KillCursors);
            stream.WriteInt32(0); // reserved
            stream.WriteInt32(message.CursorIds.Count);
            foreach (var cursorId in message.CursorIds)
            {
                stream.WriteInt64(cursorId);
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
            WriteMessage((KillCursorsMessage)message);
        }
    }
}
