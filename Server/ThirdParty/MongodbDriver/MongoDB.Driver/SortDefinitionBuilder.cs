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
    /// Extension methods for SortDefinition.
    /// </summary>
    public static class SortDefinitionExtensions
    {
        /// <summary>
        /// Combines an existing sort with an ascending field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="sort">The sort.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined sort.
        /// </returns>
        public static SortDefinition<TDocument> Ascending<TDocument>(this SortDefinition<TDocument> sort, FieldDefinition<TDocument> field)
        {
            var builder = Builders<TDocument>.Sort;
            return builder.Combine(sort, builder.Ascending(field));
        }

        /// <summary>
        /// Combines an existing sort with an ascending field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="sort">The sort.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined sort.
        /// </returns>
        public static SortDefinition<TDocument> Ascending<TDocument>(this SortDefinition<TDocument> sort, Expression<Func<TDocument, object>> field)
        {
            var builder = Builders<TDocument>.Sort;
            return builder.Combine(sort, builder.Ascending(field));
        }

        /// <summary>
        /// Combines an existing sort with an descending field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="sort">The sort.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined sort.
        /// </returns>
        public static SortDefinition<TDocument> Descending<TDocument>(this SortDefinition<TDocument> sort, FieldDefinition<TDocument> field)
        {
            var builder = Builders<TDocument>.Sort;
            return builder.Combine(sort, builder.Descending(field));
        }

        /// <summary>
        /// Combines an existing sort with an descending field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="sort">The sort.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined sort.
        /// </returns>
        public static SortDefinition<TDocument> Descending<TDocument>(this SortDefinition<TDocument> sort, Expression<Func<TDocument, object>> field)
        {
            var builder = Builders<TDocument>.Sort;
            return builder.Combine(sort, builder.Descending(field));
        }

        /// <summary>
        /// Combines an existing sort with a descending sort on the computed relevance score of a text search.
        /// The field name should be the name of the projected relevance score field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="sort">The sort.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined sort.
        /// </returns>
        public static SortDefinition<TDocument> MetaTextScore<TDocument>(this SortDefinition<TDocument> sort, string field)
        {
            var builder = Builders<TDocument>.Sort;
            return builder.Combine(sort, builder.MetaTextScore(field));
        }
    }

    /// <summary>
    /// A builder for a <see cref="SortDefinition{TDocument}"/>.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class SortDefinitionBuilder<TDocument>
    {
        /// <summary>
        /// Creates an ascending sort.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>An ascending sort.</returns>
        public SortDefinition<TDocument> Ascending(FieldDefinition<TDocument> field)
        {
            return new DirectionalSortDefinition<TDocument>(field, SortDirection.Ascending);
        }

        /// <summary>
        /// Creates an ascending sort.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>An ascending sort.</returns>
        public SortDefinition<TDocument> Ascending(Expression<Func<TDocument, object>> field)
        {
            return Ascending(new ExpressionFieldDefinition<TDocument>(field));
        }

        /// <summary>
        /// Creates a combined sort.
        /// </summary>
        /// <param name="sorts">The sorts.</param>
        /// <returns>A combined sort.</returns>
        public SortDefinition<TDocument> Combine(params SortDefinition<TDocument>[] sorts)
        {
            return Combine((IEnumerable<SortDefinition<TDocument>>)sorts);
        }

        /// <summary>
        /// Creates a combined sort.
        /// </summary>
        /// <param name="sorts">The sorts.</param>
        /// <returns>A combined sort.</returns>
        public SortDefinition<TDocument> Combine(IEnumerable<SortDefinition<TDocument>> sorts)
        {
            return new CombinedSortDefinition<TDocument>(sorts);
        }

        /// <summary>
        /// Creates a descending sort.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A descending sort.</returns>
        public SortDefinition<TDocument> Descending(FieldDefinition<TDocument> field)
        {
            return new DirectionalSortDefinition<TDocument>(field, SortDirection.Descending);
        }

        /// <summary>
        /// Creates a descending sort.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A descending sort.</returns>
        public SortDefinition<TDocument> Descending(Expression<Func<TDocument, object>> field)
        {
            return Descending(new ExpressionFieldDefinition<TDocument>(field));
        }

        /// <summary>
        /// Creates a descending sort on the computed relevance score of a text search.
        /// The name of the key should be the name of the projected relevence score field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>A meta text score sort.</returns>
        public SortDefinition<TDocument> MetaTextScore(string field)
        {
            return new BsonDocumentSortDefinition<TDocument>(new BsonDocument(field, new BsonDocument("$meta", "textScore")));
        }
    }

    internal sealed class CombinedSortDefinition<TDocument> : SortDefinition<TDocument>
    {
        private readonly List<SortDefinition<TDocument>> _sorts;

        public CombinedSortDefinition(IEnumerable<SortDefinition<TDocument>> sorts)
        {
            _sorts = Ensure.IsNotNull(sorts, nameof(sorts)).ToList();
        }

        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var document = new BsonDocument();

            foreach (var sort in _sorts)
            {
                var renderedSort = sort.Render(documentSerializer, serializerRegistry);

                foreach (var element in renderedSort.Elements)
                {
                    // the last sort always wins, and we need to make sure that order is preserved.
                    document.Remove(element.Name);
                    document.Add(element);
                }
            }

            return document;
        }
    }

    internal sealed class DirectionalSortDefinition<TDocument> : SortDefinition<TDocument>
    {
        private readonly FieldDefinition<TDocument> _field;
        private readonly SortDirection _direction;

        public DirectionalSortDefinition(FieldDefinition<TDocument> field, SortDirection direction)
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
}
