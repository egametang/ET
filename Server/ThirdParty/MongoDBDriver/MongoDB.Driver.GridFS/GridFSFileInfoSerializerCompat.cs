/* Copyright 2015-present MongoDB Inc.
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

using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Driver.GridFS
{
    /// <summary>
    /// Represents a serializer for GridFSFileInfo.
    /// </summary>
    public class GridFSFileInfoSerializer : BsonDocumentBackedClassSerializer<GridFSFileInfo>
    {
        private static readonly GridFSFileInfoSerializer __instance = new GridFSFileInfoSerializer();

        /// <summary>
        /// Gets the pre-created instance.
        /// </summary>
        public static GridFSFileInfoSerializer Instance
        {
            get { return __instance; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GridFSFileInfoSerializer" /> class.
        /// </summary>
        public GridFSFileInfoSerializer()
        {
            RegisterMember("Aliases", "aliases", new ArraySerializer<string>());
            RegisterMember("ChunkSizeBytes", "chunkSize", new Int32Serializer());
            RegisterMember("ContentType", "contentType", new StringSerializer());
            RegisterMember("Filename", "filename", new StringSerializer());
            RegisterMember("IdAsBsonValue", "_id", BsonValueSerializer.Instance);
            RegisterMember("Length", "length", new Int64Serializer());
            RegisterMember("MD5", "md5", new StringSerializer());
            RegisterMember("Metadata", "metadata", BsonDocumentSerializer.Instance);
            RegisterMember("UploadDateTime", "uploadDate", new DateTimeSerializer());
        }

        /// <inheritdoc/>
        protected override GridFSFileInfo CreateInstance(BsonDocument backingDocument)
        {
            return new GridFSFileInfo(backingDocument);
        }
    }
}
