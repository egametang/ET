/* Copyright 2010-2015 MongoDB Inc.
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
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Extension methods for an index keys definition.
    /// </summary>
    public static class IndexKeysDefinitionExtensions
    {
        /// <summary>
        /// Combines an existing index keys definition with an ascending index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> Ascending<TDocument>(this IndexKeysDefinition<TDocument> keys, FieldDefinition<TDocument> field)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.Ascending(field));
        }

        /// <summary>
        /// Combines an existing index keys definition with an ascending index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> Ascending<TDocument>(this IndexKeysDefinition<TDocument> keys, Expression<Func<TDocument, object>> field)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.Ascending(field));
        }

        /// <summary>
        /// Combines an existing index keys definition with a descending index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> Descending<TDocument>(this IndexKeysDefinition<TDocument> keys, FieldDefinition<TDocument> field)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.Descending(field));
        }

        /// <summary>
        /// Combines an existing index keys definition with a descending index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> Descending<TDocument>(this IndexKeysDefinition<TDocument> keys, Expression<Func<TDocument, object>> field)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.Descending(field));
        }

        /// <summary>
        /// Combines an existing index keys definition with a 2d index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> Geo2D<TDocument>(this IndexKeysDefinition<TDocument> keys, FieldDefinition<TDocument> field)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.Geo2D(field));
        }

        /// <summary>
        /// Combines an existing index keys definition with a 2d index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> Geo2D<TDocument>(this IndexKeysDefinition<TDocument> keys, Expression<Func<TDocument, object>> field)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.Geo2D(field));
        }

        /// <summary>
        /// Combines an existing index keys definition with a geo haystack index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <param name="additionalFieldName">Name of the additional field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> GeoHaystack<TDocument>(this IndexKeysDefinition<TDocument> keys, FieldDefinition<TDocument> field, FieldDefinition<TDocument> additionalFieldName = null)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.GeoHaystack(field, additionalFieldName));
        }

        /// <summary>
        /// Combines an existing index keys definition with a geo haystack index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <param name="additionalFieldName">Name of the additional field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> GeoHaystack<TDocument>(this IndexKeysDefinition<TDocument> keys, Expression<Func<TDocument, object>> field, Expression<Func<TDocument, object>> additionalFieldName = null)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.GeoHaystack(field, additionalFieldName));
        }

        /// <summary>
        /// Combines an existing index keys definition with a 2dsphere index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> Geo2DSphere<TDocument>(this IndexKeysDefinition<TDocument> keys, FieldDefinition<TDocument> field)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.Geo2DSphere(field));
        }

        /// <summary>
        /// Combines an existing index keys definition with a 2dsphere index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> Geo2DSphere<TDocument>(this IndexKeysDefinition<TDocument> keys, Expression<Func<TDocument, object>> field)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.Geo2DSphere(field));
        }

        /// <summary>
        /// Combines an existing index keys definition with a hashed index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> Hashed<TDocument>(this IndexKeysDefinition<TDocument> keys, FieldDefinition<TDocument> field)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.Hashed(field));
        }

        /// <summary>
        /// Combines an existing index keys definition with a hashed index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> Hashed<TDocument>(this IndexKeysDefinition<TDocument> keys, Expression<Func<TDocument, object>> field)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.Hashed(field));
        }

        /// <summary>
        /// Combines an existing index keys definition with a text index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> Text<TDocument>(this IndexKeysDefinition<TDocument> keys, FieldDefinition<TDocument> field)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.Text(field));
        }

        /// <summary>
        /// Combines an existing index keys definition with a text index key definition.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="keys">The keys.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined index keys definition.
        /// </returns>
        public static IndexKeysDefinition<TDocument> Text<TDocument>(this IndexKeysDefinition<TDocument> keys, Expression<Func<TDocument, object>> field)
        {
            var builder = Builders<TDocument>.IndexKeys;
            return builder.Combine(keys, builder.Text(field));
        }
    }

    /// <summary>
    /// A builder for an <see cref="IndexKeysDefinition{TDocument}"/>.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class IndexKeysDefinitionBuilder<TDocument>
    {
        /// <summary>
        /// Creates an ascending index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>An ascending index key definition.</returns>
        public IndexKeysDefinition<TDocument> Ascending(FieldDefinition<TDocument> field)
        {
            return new DirectionalIndexKeyDefinition<TDocument>(field, SortDirection.Ascending);
        }

        /// <summary>
        /// Creates an ascending index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>An ascending index key definition.</returns>
        public IndexKeysDefinition<TDocument> Ascending(Expression<Func<TDocument, object>> field)
        {
            return Ascending(new ExpressionFieldDefinition<TDocument>(field));
        }

        /// <summary>
        /// Creates a combined index keys definition.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns>A combined index keys definition.</returns>
        public IndexKeysDefinition<TDocument> Combine(params IndexKeysDefinition<TDocument>[] keys)
        {
            return Combine((IEnumerable<IndexKeysDefinition<TDocument>>)keys);
        }

        /// <summary>
        /// Creates a combined index keys definition.
        /// </summary>
        /// <param name="keys">The keys.</param>
        /// <returns>A combined index keys definition.</returns>
        public IndexKeysDefinition<TDocument> Combine(IEnumerable<IndexKeysDefinition<TDocument>> keys)
        {
            return new CombinedIndexKeysDefinition<TDocument>(keys);
        }

        /// <summary>
        /// Creates a descending index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A descending index key definition.</returns>
        public IndexKeysDefinition<TDocument> Descending(FieldDefinition<TDocument> field)
        {
            return new DirectionalIndexKeyDefinition<TDocument>(field, SortDirection.Descending);
        }

        /// <summary>
        /// Creates a descending index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A descending index key definition.</returns>
        public IndexKeysDefinition<TDocument> Descending(Expression<Func<TDocument, object>> field)
        {
            return Descending(new ExpressionFieldDefinition<TDocument>(field));
        }

        /// <summary>
        /// Creates a 2d index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A 2d index key definition.</returns>
        public IndexKeysDefinition<TDocument> Geo2D(FieldDefinition<TDocument> field)
        {
            return new SimpleIndexKeyDefinition<TDocument>(field, "2d");
        }

        /// <summary>
        /// Creates a 2d index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A 2d index key definition.</returns>
        public IndexKeysDefinition<TDocument> Geo2D(Expression<Func<TDocument, object>> field)
        {
            return Geo2D(new ExpressionFieldDefinition<TDocument>(field));
        }

        /// <summary>
        /// Creates a geo haystack index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="additionalFieldName">Name of the additional field.</param>
        /// <returns>
        /// A geo haystack index key definition.
        /// </returns>
        public IndexKeysDefinition<TDocument> GeoHaystack(FieldDefinition<TDocument> field, FieldDefinition<TDocument> additionalFieldName = null)
        {
            return new GeoHaystackIndexKeyDefinition<TDocument>(field, additionalFieldName);
        }

        /// <summary>
        /// Creates a geo haystack index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="additionalFieldName">Name of the additional field.</param>
        /// <returns>
        /// A geo haystack index key definition.
        /// </returns>
        public IndexKeysDefinition<TDocument> GeoHaystack(Expression<Func<TDocument, object>> field, Expression<Func<TDocument, object>> additionalFieldName = null)
        {
            FieldDefinition<TDocument> additional = additionalFieldName == null ? null : new ExpressionFieldDefinition<TDocument>(additionalFieldName);
            return GeoHaystack(new ExpressionFieldDefinition<TDocument>(field), additional);
        }

        /// <summary>
        /// Creates a 2dsphere index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A 2dsphere index key definition.</returns>
        public IndexKeysDefinition<TDocument> Geo2DSphere(FieldDefinition<TDocument> field)
        {
            return new SimpleIndexKeyDefinition<TDocument>(field, "2dsphere");
        }

        /// <summary>
        /// Creates a 2dsphere index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A 2dsphere index key definition.</returns>
        public IndexKeysDefinition<TDocument> Geo2DSphere(Expression<Func<TDocument, object>> field)
        {
            return Geo2DSphere(new ExpressionFieldDefinition<TDocument>(field));
        }

        /// <summary>
        /// Creates a hashed index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A hashed index key definition.</returns>
        public IndexKeysDefinition<TDocument> Hashed(FieldDefinition<TDocument> field)
        {
            return new SimpleIndexKeyDefinition<TDocument>(field, "hashed");
        }

        /// <summary>
        /// Creates a hashed index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A hashed index key definition.</returns>
        public IndexKeysDefinition<TDocument> Hashed(Expression<Func<TDocument, object>> field)
        {
            return Hashed(new ExpressionFieldDefinition<TDocument>(field));
        }

        /// <summary>
        /// Creates a text index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A text index key definition.</returns>
        public IndexKeysDefinition<TDocument> Text(FieldDefinition<TDocument> field)
        {
            return new SimpleIndexKeyDefinition<TDocument>(field, "text");
        }

        /// <summary>
        /// Creates a text index key definition.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A text index key definition.</returns>
        public IndexKeysDefinition<TDocument> Text(Expression<Func<TDocument, object>> field)
        {
            return Text(new ExpressionFieldDefinition<TDocument>(field));
        }
    }

    internal sealed class CombinedIndexKeysDefinition<TDocument> : IndexKeysDefinition<TDocument>
    {
        private readonly List<IndexKeysDefinition<TDocument>> _keys;

        public CombinedIndexKeysDefinition(IEnumerable<IndexKeysDefinition<TDocument>> keys)
        {
            _keys = Ensure.IsNotNull(keys, nameof(keys)).ToList();
        }

        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var document = new BsonDocument();

            foreach (var key in _keys)
            {
                var renderedKey = key.Render(documentSerializer, serializerRegistry);

                foreach (var element in renderedKey.Elements)
                {
                    if (document.Contains(element.Name))
                    {
                        var message = string.Format(
                            "The index keys definition contains multiple values for the field '{0}'.",
                            element.Name);
                        throw new MongoException(message);
                    }
                    document.Add(element);
                }
            }

            return document;
        }
    }

    internal sealed class DirectionalIndexKeyDefinition<TDocument> : IndexKeysDefinition<TDocument>
    {
        private readonly FieldDefinition<TDocument> _field;
        private readonly SortDirection _direction;

        public DirectionalIndexKeyDefinition(FieldDefinition<TDocument> field, SortDirection direction)
        {
            _field = Ensure.IsNotNull(field, nameof(field));
            _direction = direction;
        }

        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedField = _field.Render(documentSerializer, serializerRegistry);

            BsonValue value;
            switch (_direction)
            {
                case SortDirection.Ascending:
                    value = 1;
                    break;
                case SortDirection.Descending:
                    value = -1;
                    break;
                default:
                    throw new InvalidOperationException("Unknown value for " + typeof(SortDirection) + ".");
            }

            return new BsonDocument(renderedField.FieldName, value);
        }
    }

    internal sealed class GeoHaystackIndexKeyDefinition<TDocument> : IndexKeysDefinition<TDocument>
    {
        private readonly FieldDefinition<TDocument> _field;
        private readonly FieldDefinition<TDocument> _additionalFieldName;

        public GeoHaystackIndexKeyDefinition(FieldDefinition<TDocument> field, FieldDefinition<TDocument> additionalFieldName = null)
        {
            _field = Ensure.IsNotNull(field, nameof(field));
            _additionalFieldName = additionalFieldName;
        }

        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedField = _field.Render(documentSerializer, serializerRegistry);

            var document = new BsonDocument(renderedField.FieldName, "geoHaystack");
            if (_additionalFieldName != null)
            {
                var additionalRenderedField = _additionalFieldName.Render(documentSerializer, serializerRegistry);
                document.Add(additionalRenderedField.FieldName, 1);
            }

            return document;
        }
    }

    internal sealed class SimpleIndexKeyDefinition<TDocument> : IndexKeysDefinition<TDocument>
    {
        private readonly FieldDefinition<TDocument> _field;
        private readonly string _type;

        public SimpleIndexKeyDefinition(FieldDefinition<TDocument> field, string type)
        {
            _field = Ensure.IsNotNull(field, nameof(field));
            _type = type;
        }

        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedField = _field.Render(documentSerializer, serializerRegistry);
            return new BsonDocument(renderedField.FieldName, _type);
        }
    }
}
