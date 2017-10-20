/* Copyright 2015 MongoDB Inc.
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

using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.WireProtocol.Messages.Encoders;

namespace MongoDB.Driver.Core.Operations
{
    /// <summary>
    /// A helper class for deserializing documents in a cursor batch.
    /// </summary>
    internal static class CursorBatchDeserializationHelper
    {
        // public methods
        /// <summary>
        /// Deserializes the documents.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="batch">The batch.</param>
        /// <param name="documentSerializer">The document serializer.</param>
        /// <param name="messageEncoderSettings">The message encoder settings.</param>
        /// <returns>The documents.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times")]
        public static List<TDocument> DeserializeBatch<TDocument>(RawBsonArray batch, IBsonSerializer<TDocument> documentSerializer, MessageEncoderSettings messageEncoderSettings)
        {
            var documents = new List<TDocument>();

            var readerSettings = new BsonBinaryReaderSettings();
            if (messageEncoderSettings != null)
            {
                readerSettings.Encoding = messageEncoderSettings.GetOrDefault(MessageEncoderSettingsName.ReadEncoding, Utf8Encodings.Strict);
                readerSettings.GuidRepresentation = messageEncoderSettings.GetOrDefault(MessageEncoderSettingsName.GuidRepresentation, GuidRepresentation.CSharpLegacy);
            };

            using (var stream = new ByteBufferStream(batch.Slice, ownsBuffer: false))
            using (var reader = new BsonBinaryReader(stream, readerSettings))
            {
                // BSON requires that the top level object be a document, but an array looks close enough to a document that we can pretend it is one
                reader.ReadStartDocument();
                while (reader.ReadBsonType() != 0)
                {
                    reader.SkipName(); // skip over the index pseudo names
                    var context = BsonDeserializationContext.CreateRoot(reader);
                    var document = documentSerializer.Deserialize<TDocument>(context);
                    documents.Add(document);
                }
                reader.ReadEndDocument();
            }

            return documents;
        }
    }
}
