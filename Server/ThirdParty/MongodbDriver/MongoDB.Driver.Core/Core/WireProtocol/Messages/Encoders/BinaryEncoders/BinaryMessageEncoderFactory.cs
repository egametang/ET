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

using System.IO;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders.BinaryEncoders
{
    /// <summary>
    /// Represents a factory for binary message encoders.
    /// </summary>
    public class BinaryMessageEncoderFactory : IMessageEncoderFactory
    {
        // fields
        private readonly MessageEncoderSettings _encoderSettings;
        private readonly Stream _stream;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryMessageEncoderFactory"/> class.
        /// </summary>
        /// <param name="stream">The stream.</param>
        /// <param name="encoderSettings">The encoder settings.</param>
        public BinaryMessageEncoderFactory(Stream stream, MessageEncoderSettings encoderSettings)
        {
            _stream = Ensure.IsNotNull(stream, nameof(stream));
            _encoderSettings = encoderSettings; // can be null
        }

        // methods
        /// <inheritdoc/>
        public IMessageEncoder GetDeleteMessageEncoder()
        {
            return new DeleteMessageBinaryEncoder(_stream, _encoderSettings);
        }

        /// <inheritdoc/>
        public IMessageEncoder GetGetMoreMessageEncoder()
        {
            return new GetMoreMessageBinaryEncoder(_stream, _encoderSettings);
        }

        /// <inheritdoc/>
        public IMessageEncoder GetInsertMessageEncoder<TDocument>(IBsonSerializer<TDocument> serializer)
        {
            return new InsertMessageBinaryEncoder<TDocument>(_stream, _encoderSettings, serializer);
        }

        /// <inheritdoc/>
        public IMessageEncoder GetKillCursorsMessageEncoder()
        {
            return new KillCursorsMessageBinaryEncoder(_stream, _encoderSettings);
        }

        /// <inheritdoc/>
        public IMessageEncoder GetQueryMessageEncoder()
        {
            return new QueryMessageBinaryEncoder(_stream, _encoderSettings);
        }

        /// <inheritdoc/>
        public IMessageEncoder GetReplyMessageEncoder<TDocument>(IBsonSerializer<TDocument> serializer)
        {
            return new ReplyMessageBinaryEncoder<TDocument>(_stream, _encoderSettings, serializer);
        }

        /// <inheritdoc/>
        public IMessageEncoder GetUpdateMessageEncoder()
        {
            return new UpdateMessageBinaryEncoder(_stream, _encoderSettings);
        }
    }
}
