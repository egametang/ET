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

using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders
{
    /// <summary>
    /// Represents a message encoder selector for ReplyMessages.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public class ReplyMessageEncoderSelector<TDocument> : IMessageEncoderSelector
    {
        // fields
        private readonly IBsonSerializer<TDocument> _documentSerializer;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReplyMessageEncoderSelector{TDocument}"/> class.
        /// </summary>
        /// <param name="documentSerializer">The document serializer.</param>
        public ReplyMessageEncoderSelector(IBsonSerializer<TDocument> documentSerializer)
        {
            _documentSerializer = Ensure.IsNotNull(documentSerializer, nameof(documentSerializer));
        }

        // methods        
        /// <inheritdoc />
        public IMessageEncoder GetEncoder(IMessageEncoderFactory encoderFactory)
        {
            return encoderFactory.GetReplyMessageEncoder<TDocument>(_documentSerializer);
        }
    }
}
