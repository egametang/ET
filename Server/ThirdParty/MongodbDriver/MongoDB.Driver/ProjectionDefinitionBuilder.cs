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
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver
{
    /// <summary>
    /// Extension methods for projections.
    /// </summary>
    public static class ProjectionDefinitionExtensions
    {
        /// <summary>
        /// Combines an existing projection with a projection that filters the contents of an array.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// A combined projection.
        /// </returns>
        public static ProjectionDefinition<TDocument> ElemMatch<TDocument, TItem>(this ProjectionDefinition<TDocument> projection, FieldDefinition<TDocument> field, FilterDefinition<TItem> filter)
        {
            var builder = Builders<TDocument>.Projection;
            return builder.Combine(projection, builder.ElemMatch(field, filter));
        }

        /// <summary>
        /// Combines an existing projection with a projection that filters the contents of an array.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// A combined projection.
        /// </returns>
        public static ProjectionDefinition<TDocument> ElemMatch<TDocument, TItem>(this ProjectionDefinition<TDocument> projection, Expression<Func<TDocument, IEnumerable<TItem>>> field, FilterDefinition<TItem> filter)
        {
            var builder = Builders<TDocument>.Projection;
            return builder.Combine(projection, builder.ElemMatch(field, filter));
        }

        /// <summary>
        /// Combines an existing projection with a projection that filters the contents of an array.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// A combined projection.
        /// </returns>
        public static ProjectionDefinition<TDocument> ElemMatch<TDocument, TItem>(this ProjectionDefinition<TDocument> projection, Expression<Func<TDocument, IEnumerable<TItem>>> field, Expression<Func<TItem, bool>> filter)
        {
            var builder = Builders<TDocument>.Projection;
            return builder.Combine(projection, builder.ElemMatch(field, filter));
        }

        /// <summary>
        /// Combines an existing projection with a projection that excludes a field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined projection.
        /// </returns>
        public static ProjectionDefinition<TDocument> Exclude<TDocument>(this ProjectionDefinition<TDocument> projection, FieldDefinition<TDocument> field)
        {
            var builder = Builders<TDocument>.Projection;
            return builder.Combine(projection, builder.Exclude(field));
        }

        /// <summary>
        /// Combines an existing projection with a projection that excludes a field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined projection.
        /// </returns>
        public static ProjectionDefinition<TDocument> Exclude<TDocument>(this ProjectionDefinition<TDocument> projection, Expression<Func<TDocument, object>> field)
        {
            var builder = Builders<TDocument>.Projection;
            return builder.Combine(projection, builder.Exclude(field));
        }

        /// <summary>
        /// Combines an existing projection with a projection that includes a field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined projection.
        /// </returns>
        public static ProjectionDefinition<TDocument> Include<TDocument>(this ProjectionDefinition<TDocument> projection, FieldDefinition<TDocument> field)
        {
            var builder = Builders<TDocument>.Projection;
            return builder.Combine(projection, builder.Include(field));
        }

        /// <summary>
        /// Combines an existing projection with a projection that includes a field.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined projection.
        /// </returns>
        public static ProjectionDefinition<TDocument> Include<TDocument>(this ProjectionDefinition<TDocument> projection, Expression<Func<TDocument, object>> field)
        {
            var builder = Builders<TDocument>.Projection;
            return builder.Combine(projection, builder.Include(field));
        }

        /// <summary>
        /// Combines an existing projection with a text score projection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A combined projection.
        /// </returns>
        public static ProjectionDefinition<TDocument> MetaTextScore<TDocument>(this ProjectionDefinition<TDocument> projection, string field)
        {
            var builder = Builders<TDocument>.Projection;
            return builder.Combine(projection, builder.MetaTextScore(field));
        }

        /// <summary>
        /// Combines an existing projection with an array slice projection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="field">The field.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>
        /// A combined projection.
        /// </returns>
        public static ProjectionDefinition<TDocument> Slice<TDocument>(this ProjectionDefinition<TDocument> projection, FieldDefinition<TDocument> field, int skip, int? limit = null)
        {
            var builder = Builders<TDocument>.Projection;
            return builder.Combine(projection, builder.Slice(field, skip, limit));
        }

        /// <summary>
        /// Combines an existing projection with an array slice projection.
        /// </summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="projection">The projection.</param>
        /// <param name="field">The field.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>
        /// A combined projection.
        /// </returns>
        public static ProjectionDefinition<TDocument> Slice<TDocument>(this ProjectionDefinition<TDocument> projection, Expression<Func<TDocument, object>> field, int skip, int? limit = null)
        {
            var builder = Builders<TDocument>.Projection;
            return builder.Combine(projection, builder.Slice(field, skip, limit));
        }
    }

    /// <summary>
    /// A builder for a projection.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public sealed class ProjectionDefinitionBuilder<TSource>
    {
        /// <summary>
        /// Creates a client side projection that is implemented solely by using a different serializer.
        /// </summary>
        /// <typeparam name="TProjection">The type of the projection.</typeparam>
        /// <param name="projectionSerializer">The projection serializer.</param>
        /// <returns>A client side deserialization projection.</returns>
        public ProjectionDefinition<TSource, TProjection> As<TProjection>(IBsonSerializer<TProjection> projectionSerializer = null)
        {
            return new ClientSideDeserializationProjectionDefinition<TSource, TProjection>(projectionSerializer);
        }

        /// <summary>
        /// Combines the specified projections.
        /// </summary>
        /// <param name="projections">The projections.</param>
        /// <returns>
        /// A combined projection.
        /// </returns>
        public ProjectionDefinition<TSource> Combine(params ProjectionDefinition<TSource>[] projections)
        {
            return Combine((IEnumerable<ProjectionDefinition<TSource>>)projections);
        }

        /// <summary>
        /// Combines the specified projections.
        /// </summary>
        /// <param name="projections">The projections.</param>
        /// <returns>
        /// A combined projection.
        /// </returns>
        public ProjectionDefinition<TSource> Combine(IEnumerable<ProjectionDefinition<TSource>> projections)
        {
            return new CombinedProjectionDefinition<TSource>(projections);
        }

        /// <summary>
        /// Creates a projection that filters the contents of an array.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// An array filtering projection.
        /// </returns>
        public ProjectionDefinition<TSource> ElemMatch<TItem>(FieldDefinition<TSource> field, FilterDefinition<TItem> filter)
        {
            return new ElementMatchProjectionDefinition<TSource, TItem>(field, filter);
        }

        /// <summary>
        /// Creates a projection that filters the contents of an array.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// An array filtering projection.
        /// </returns>
        public ProjectionDefinition<TSource> ElemMatch<TItem>(Expression<Func<TSource, IEnumerable<TItem>>> field, FilterDefinition<TItem> filter)
        {
            return ElemMatch(new ExpressionFieldDefinition<TSource>(field), filter);
        }

        /// <summary>
        /// Creates a projection that filters the contents of an array.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="field">The field.</param>
        /// <param name="filter">The filter.</param>
        /// <returns>
        /// An array filtering projection.
        /// </returns>
        public ProjectionDefinition<TSource> ElemMatch<TItem>(Expression<Func<TSource, IEnumerable<TItem>>> field, Expression<Func<TItem, bool>> filter)
        {
            return ElemMatch(new ExpressionFieldDefinition<TSource>(field), new ExpressionFilterDefinition<TItem>(filter));
        }

        /// <summary>
        /// Creates a projection that excludes a field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>
        /// An exclusion projection.
        /// </returns>
        public ProjectionDefinition<TSource> Exclude(FieldDefinition<TSource> field)
        {
            return new SingleFieldProjectionDefinition<TSource>(field, 0);
        }

        /// <summary>
        /// Creates a projection that excludes a field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>
        /// An exclusion projection.
        /// </returns>
        public ProjectionDefinition<TSource> Exclude(Expression<Func<TSource, object>> field)
        {
            return Exclude(new ExpressionFieldDefinition<TSource>(field));
        }

        /// <summary>
        /// Creates a projection based on the expression.
        /// </summary>
        /// <typeparam name="TProjection">The type of the result.</typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// An expression projection.
        /// </returns>
        public ProjectionDefinition<TSource, TProjection> Expression<TProjection>(Expression<Func<TSource, TProjection>> expression)
        {
            return new FindExpressionProjectionDefinition<TSource, TProjection>(expression);
        }

        /// <summary>
        /// Creates a projection that includes a field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>
        /// An inclusion projection.
        /// </returns>
        public ProjectionDefinition<TSource> Include(FieldDefinition<TSource> field)
        {
            return new SingleFieldProjectionDefinition<TSource>(field, 1);
        }

        /// <summary>
        /// Creates a projection that includes a field.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>
        /// An inclusion projection.
        /// </returns>
        public ProjectionDefinition<TSource> Include(Expression<Func<TSource, object>> field)
        {
            return Include(new ExpressionFieldDefinition<TSource>(field));
        }

        /// <summary>
        /// Creates a text score projection.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <returns>
        /// A text score projection.
        /// </returns>
        public ProjectionDefinition<TSource> MetaTextScore(string field)
        {
            return new SingleFieldProjectionDefinition<TSource>(field, new BsonDocument("$meta", "textScore"));
        }

        /// <summary>
        /// Creates an array slice projection.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>
        /// An array slice projection.
        /// </returns>
        public ProjectionDefinition<TSource> Slice(FieldDefinition<TSource> field, int skip, int? limit = null)
        {
            var value = limit.HasValue ? (BsonValue)new BsonArray { skip, limit.Value } : skip;
            return new SingleFieldProjectionDefinition<TSource>(field, new BsonDocument("$slice", value));
        }

        /// <summary>
        /// Creates an array slice projection.
        /// </summary>
        /// <param name="field">The field.</param>
        /// <param name="skip">The skip.</param>
        /// <param name="limit">The limit.</param>
        /// <returns>
        /// An array slice projection.
        /// </returns>
        public ProjectionDefinition<TSource> Slice(Expression<Func<TSource, object>> field, int skip, int? limit = null)
        {
            return Slice(new ExpressionFieldDefinition<TSource>(field), skip, limit);
        }
    }

    internal sealed class CombinedProjectionDefinition<TSource> : ProjectionDefinition<TSource>
    {
        private readonly List<ProjectionDefinition<TSource>> _projections;

        public CombinedProjectionDefinition(IEnumerable<ProjectionDefinition<TSource>> projections)
        {
            _projections = Ensure.IsNotNull(projections, nameof(projections)).ToList();
        }

        public override BsonDocument Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var document = new BsonDocument();

            foreach (var projection in _projections)
            {
                var renderedProjection = projection.Render(sourceSerializer, serializerRegistry);

                foreach (var element in renderedProjection.Elements)
                {
                    // last one wins
                    document.Remove(element.Name);
                    document.Add(element);
                }
            }

            return document;
        }
    }

    internal sealed class ElementMatchProjectionDefinition<TSource, TItem> : ProjectionDefinition<TSource>
    {
        private readonly FieldDefinition<TSource> _field;
        private readonly FilterDefinition<TItem> _filter;

        public ElementMatchProjectionDefinition(FieldDefinition<TSource> field, FilterDefinition<TItem> filter)
        {
            _field = Ensure.IsNotNull(field, nameof(field));
            _filter = filter;
        }

        public override BsonDocument Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedField = _field.Render(sourceSerializer, serializerRegistry);

            IBsonSerializer<TItem> itemSerializer;
            if (renderedField.FieldSerializer != null)
            {
                var arraySerializer = renderedField.FieldSerializer as IBsonArraySerializer;
                BsonSerializationInfo itemSerializationInfo;
                if (arraySerializer == null || !arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                {
                    var message = string.Format("The serializer for field '{0}' must implement IBsonArraySerializer and provide item serialization info.", renderedField.FieldName);
                    throw new InvalidOperationException(message);
                }
                itemSerializer = (IBsonSerializer<TItem>)itemSerializationInfo.Serializer;
            }
            else
            {
                itemSerializer = serializerRegistry.GetSerializer<TItem>();
            }

            var renderedFilter = _filter.Render(itemSerializer, serializerRegistry);

            return new BsonDocument(renderedField.FieldName, new BsonDocument("$elemMatch", renderedFilter));
        }
    }

    internal sealed class PositionalOperatorProjectionDefinition<TSource> : ProjectionDefinition<TSource>
    {
        private readonly FieldDefinition<TSource> _field;

        public PositionalOperatorProjectionDefinition(FieldDefinition<TSource> field)
        {
            _field = Ensure.IsNotNull(field, nameof(field));
        }

        public override BsonDocument Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedField = _field.Render(sourceSerializer, serializerRegistry);
            return new BsonDocument(renderedField.FieldName + ".$", 1);
        }
    }

    internal sealed class SingleFieldProjectionDefinition<TSource> : ProjectionDefinition<TSource>
    {
        private readonly FieldDefinition<TSource> _field;
        private readonly BsonValue _value;

        public SingleFieldProjectionDefinition(FieldDefinition<TSource> field, BsonValue value)
        {
            _field = Ensure.IsNotNull(field, nameof(field));
            _value = Ensure.IsNotNull(value, nameof(value));
        }

        public override BsonDocument Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var renderedField = _field.Render(sourceSerializer, serializerRegistry);
            return new BsonDocument(renderedField.FieldName, _value);
        }
    }
}
