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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.Core.WireProtocol.Messages.Encoders
{
    /// <summary>
    /// Represents a message encoder factory.
    /// </summary>
    public interface IMessageEncoderFactory
    {
        /// <summary>
        /// Gets an encoder for a Delete message.
        /// </summary>
        /// <returns>An encoder.</returns>
        IMessageEncoder GetDeleteMessageEncoder();

        /// <summary>
        /// Gets an encoder for a GetMore message.
        /// </summary>
        /// <returns>An encoder.</returns>
        IMessageEncoder GetGetMoreMessageEncoder();

        /// <summary>
        /// Gets an encoder for an Insert message.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <returns>An encoder.</returns>
        IMessageEncoder GetInsertMessageEncoder<TDocument>(IBsonSerializer<TDocument> serializer);

        /// <summary>
        /// Gets an encoder for a KillCursors message.
        /// </summary>
        /// <returns>An encoder.</returns>
        IMessageEncoder GetKillCursorsMessageEncoder();

        /// <summary>
        /// Gets an encoder for a Query message.
        /// </summary>
        /// <returns>An encoder.</returns>
        IMessageEncoder GetQueryMessageEncoder();

        /// <summary>
        /// Gets an encoder for a Reply message.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="serializer">The serializer.</param>
        /// <returns>An encoder.</returns>
        IMessageEncoder GetReplyMessageEncoder<TDocument>(IBsonSerializer<TDocument> serializer);

        /// <summary>
        /// Gets an encoder for an Update message.
        /// </summary>
        /// <returns>An encoder.</returns>
        IMessageEncoder GetUpdateMessageEncoder();
    }
}
