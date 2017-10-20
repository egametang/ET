/* Copyright 2017 MongoDB Inc.
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
using MongoDB.Driver.Core.Misc;
using System;

namespace MongoDB.Driver
{
    /// <summary>
    /// Base class for array filters.
    /// </summary>
    public abstract class ArrayFilterDefinition
    {
        // public properties
        /// <summary>
        /// Gets the type of an item.
        /// </summary>
        /// <value>
        /// The type of an item.
        /// </value>
        public abstract Type ItemType { get; }

        // public methods
        /// <summary>
        /// Renders the array filter to a <see cref="BsonDocument" />.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>
        /// A <see cref="BsonDocument" />.
        /// </returns>
        public abstract BsonDocument Render(IBsonSerializer itemSerializer, IBsonSerializerRegistry serializerRegistry);
    }

    /// <summary>
    /// Base class for array filters.
    /// </summary>
    /// <typeparam name="TItem">The type of an item.</typeparam>
    public abstract class ArrayFilterDefinition<TItem> : ArrayFilterDefinition
    {
        // public properties
        /// <inheritdoc />
        public override Type ItemType => typeof(TItem);

        // implicit conversions
        /// <summary>
        /// Performs an implicit conversion from <see cref="BsonDocument"/> to <see cref="ArrayFilterDefinition{TItem}"/>.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ArrayFilterDefinition<TItem>(BsonDocument document)
        {
            if (document == null)
            {
                return null;
            }

            return new BsonDocumentArrayFilterDefinition<TItem>(document);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="ArrayFilterDefinition{TItem}"/>.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ArrayFilterDefinition<TItem>(string json)
        {
            if (json == null)
            {
                return null;
            }

            return new JsonArrayFilterDefinition<TItem>(json);
        }

        // public methods
        /// <summary>
        /// Renders the array filter to a <see cref="BsonDocument" />.
        /// </summary>
        /// <param name="itemSerializer">The item serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>
        /// A <see cref="BsonDocument" />.
        /// </returns>
        public abstract BsonDocument Render(IBsonSerializer<TItem> itemSerializer, IBsonSerializerRegistry serializerRegistry);
    }

    /// <summary>
    /// A <see cref="BsonDocument"/> based array filter.
    /// </summary>
    /// <typeparam name="TItem">The type of an item.</typeparam>
    public sealed class BsonDocumentArrayFilterDefinition<TItem> : ArrayFilterDefinition<TItem>
    {
        // private fields
        private readonly BsonDocument _document;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentArrayFilterDefinition{TItem}"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public BsonDocumentArrayFilterDefinition(BsonDocument document)
        {
            _document = Ensure.IsNotNull(document, nameof(document));
        }

        // public properties
        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public BsonDocument Document => _document;

        // public methods
        /// <inheritdoc />
        public override BsonDocument Render(IBsonSerializer itemSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return Render((IBsonSerializer<TItem>) itemSerializer, serializerRegistry);
        }

        /// <inheritdoc />
        public override BsonDocument Render(IBsonSerializer<TItem> itemSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return _document;
        }
    }

    /// <summary>
    /// A JSON <see cref="String"/> based array filter.
    /// </summary>
    /// <typeparam name="TItem">The type of an item.</typeparam>
    public sealed class JsonArrayFilterDefinition<TItem> : ArrayFilterDefinition<TItem>
    {
        // private fields
        private readonly BsonDocument _document;
        private readonly string _json;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentArrayFilterDefinition{TItem}"/> class.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        public JsonArrayFilterDefinition(string json)
        {
            _json = Ensure.IsNotNull(json, nameof(json));
            _document = BsonDocument.Parse(json);
        }

        // public properties
        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <value>
        /// The document.
        /// </value>
        public BsonDocument Document => _document;

        /// <summary>
        /// Gets the JSON string.
        /// </summary>
        /// <value>
        /// The JSON string.
        /// </value>
        public string Json => _json;

        // public methods
        /// <inheritdoc />
        public override BsonDocument Render(IBsonSerializer itemSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return Render((IBsonSerializer<TItem>)itemSerializer, serializerRegistry);
        }

        /// <inheritdoc />
        public override BsonDocument Render(IBsonSerializer<TItem> itemSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return _document;
        }
    }
}
