/* Copyright 2010-present MongoDB Inc.
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
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// The direction of the sort.
    /// </summary>
    public enum SortDirection
    {
        /// <summary>
        /// Ascending.
        /// </summary>
        Ascending,
        /// <summary>
        /// Descending.
        /// </summary>
        Descending
    }

    /// <summary>
    /// Base class for sorts.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public abstract class SortDefinition<TDocument>
    {
        /// <summary>
        /// Renders the sort to a <see cref="BsonDocument"/>.
        /// </summary>
        /// <param name="documentSerializer">The document serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>A <see cref="BsonDocument"/>.</returns>
        public abstract BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry);

        /// <summary>
        /// Performs an implicit conversion from <see cref="BsonDocument"/> to <see cref="SortDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator SortDefinition<TDocument>(BsonDocument document)
        {
            if (document == null)
            {
                return null;
            }

            return new BsonDocumentSortDefinition<TDocument>(document);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="SortDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator SortDefinition<TDocument>(string json)
        {
            if (json == null)
            {
                return null;
            }

            return new JsonSortDefinition<TDocument>(json);
        }
    }

    /// <summary>
    /// A <see cref="BsonDocument"/> based sort.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class BsonDocumentSortDefinition<TDocument> : SortDefinition<TDocument>
    {
        private readonly BsonDocument _document;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentSortDefinition{TDocument}"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public BsonDocumentSortDefinition(BsonDocument document)
        {
            _document = Ensure.IsNotNull(document, nameof(document));
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        public BsonDocument Document
        {
            get { return _document; }
        }

        /// <inheritdoc />
        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return _document;
        }
    }

    /// <summary>
    /// A JSON <see cref="String" /> based sort.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class JsonSortDefinition<TDocument> : SortDefinition<TDocument>
    {
        private readonly string _json;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonSortDefinition{TDocument}"/> class.
        /// </summary>
        /// <param name="json">The json.</param>
        public JsonSortDefinition(string json)
        {
            _json = Ensure.IsNotNullOrEmpty(json, nameof(json));
        }

        /// <summary>
        /// Gets the json.
        /// </summary>
        public string Json
        {
            get { return _json; }
        }

        /// <inheritdoc />
        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return BsonDocument.Parse(_json);
        }
    }

    /// <summary>
    /// An <see cref="Object" /> based sort.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class ObjectSortDefinition<TDocument> : SortDefinition<TDocument>
    {
        private readonly object _obj;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectSortDefinition{TDocument}"/> class.
        /// </summary>
        /// <param name="obj">The object.</param>
        public ObjectSortDefinition(object obj)
        {
            _obj = Ensure.IsNotNull(obj, nameof(obj));
        }

        /// <summary>
        /// Gets the object.
        /// </summary>
        public object Object
        {
            get { return _obj; }
        }

        /// <inheritdoc />
        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var serializer = serializerRegistry.GetSerializer(_obj.GetType());
            return new BsonDocumentWrapper(_obj, serializer);
        }
    }
}
