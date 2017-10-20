/* Copyright 2016 MongoDB Inc.
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

namespace MongoDB.Driver
{
    /// <summary>
    /// Options for creating a view.
    /// </summary>
    /// <typeparam name="TDocument">The type of the documents.</typeparam>
    public class CreateViewOptions<TDocument>
    {
        // fields
        private Collation _collation;
        private IBsonSerializer<TDocument> _documentSerializer;
        private IBsonSerializerRegistry _serializerRegistry;

        // properties
        /// <summary>
        /// Gets or sets the collation.
        /// </summary>
        /// <value>
        /// The collation.
        /// </value>
        public Collation Collation
        {
            get { return _collation; }
            set { _collation = value; }
        }

        /// <summary>
        /// Gets or sets the document serializer.
        /// </summary>
        /// <value>
        /// The document serializer.
        /// </value>
        public IBsonSerializer<TDocument> DocumentSerializer
        {
            get { return _documentSerializer; }
            set { _documentSerializer = value; }
        }

        /// <summary>
        /// Gets or sets the serializer registry.
        /// </summary>
        /// <value>
        /// The serializer registry.
        /// </value>
        public IBsonSerializerRegistry SerializerRegistry
        {
            get { return _serializerRegistry; }
            set { _serializerRegistry = value; }
        }
    }
}
