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
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Translators;

namespace MongoDB.Driver
{
    /// <summary>
    /// A rendered projection.
    /// </summary>
    /// <typeparam name="TProjection">The type of the projection.</typeparam>
    public sealed class RenderedProjectionDefinition<TProjection>
    {
        private readonly BsonDocument _projection;
        private readonly IBsonSerializer<TProjection> _projectionSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderedProjectionDefinition{TProjection}" /> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="projectionSerializer">The projection serializer.</param>
        public RenderedProjectionDefinition(BsonDocument document, IBsonSerializer<TProjection> projectionSerializer)
        {
            _projection = document;
            _projectionSerializer = Ensure.IsNotNull(projectionSerializer, nameof(projectionSerializer));
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        public BsonDocument Document
        {
            get { return _projection; }
        }

        /// <summary>
        /// Gets the serializer.
        /// </summary>
        public IBsonSerializer<TProjection> ProjectionSerializer
        {
            get { return _projectionSerializer; }
        }
    }

    /// <summary>
    /// Base class for projections whose projection type is not yet known.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public abstract class ProjectionDefinition<TSource>
    {
        /// <summary>
        /// Renders the projection to a <see cref="RenderedProjectionDefinition{TProjection}"/>.
        /// </summary>
        /// <param name="sourceSerializer">The source serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>A <see cref="BsonDocument"/>.</returns>
        public abstract BsonDocument Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry);

        /// <summary>
        /// Performs an implicit conversion from <see cref="BsonDocument"/> to <see cref="ProjectionDefinition{TSource}"/>.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ProjectionDefinition<TSource>(BsonDocument document)
        {
            if (document == null)
            {
                return null;
            }

            return new BsonDocumentProjectionDefinition<TSource>(document);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String" /> to <see cref="ProjectionDefinition{TSource, TProjection}" />.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ProjectionDefinition<TSource>(string json)
        {
            if (json == null)
            {
                return null;
            }

            return new JsonProjectionDefinition<TSource>(json);
        }
    }

    /// <summary>
    /// Base class for projections.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TProjection">The type of the projection.</typeparam>
    public abstract class ProjectionDefinition<TSource, TProjection>
    {
        /// <summary>
        /// Renders the projection to a <see cref="RenderedProjectionDefinition{TProjection}"/>.
        /// </summary>
        /// <param name="sourceSerializer">The source serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>A <see cref="RenderedProjectionDefinition{TProjection}"/>.</returns>
        public abstract RenderedProjectionDefinition<TProjection> Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry);

        /// <summary>
        /// Performs an implicit conversion from <see cref="BsonDocument"/> to <see cref="ProjectionDefinition{TSource, TProjection}"/>.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ProjectionDefinition<TSource, TProjection>(BsonDocument document)
        {
            if (document == null)
            {
                return null;
            }

            return new BsonDocumentProjectionDefinition<TSource, TProjection>(document);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String" /> to <see cref="ProjectionDefinition{TSource, TProjection}" />.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ProjectionDefinition<TSource, TProjection>(string json)
        {
            if (json == null)
            {
                return null;
            }

            return new JsonProjectionDefinition<TSource, TProjection>(json);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="ProjectionDefinition{TSource}"/> to <see cref="ProjectionDefinition{TSource, TProjection}"/>.
        /// </summary>
        /// <param name="projection">The projection.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator ProjectionDefinition<TSource, TProjection>(ProjectionDefinition<TSource> projection)
        {
            return new KnownResultTypeProjectionDefinitionAdapter<TSource, TProjection>(projection);
        }
    }

    /// <summary>
    /// A <see cref="BsonDocument" /> based projection whose projection type is not yet known.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public sealed class BsonDocumentProjectionDefinition<TSource> : ProjectionDefinition<TSource>
    {
        private readonly BsonDocument _document;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentProjectionDefinition{TSource}"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public BsonDocumentProjectionDefinition(BsonDocument document)
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
        public override BsonDocument Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return _document;
        }
    }

    /// <summary>
    /// A <see cref="BsonDocument" /> based projection.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TProjection">The type of the projection.</typeparam>
    public sealed class BsonDocumentProjectionDefinition<TSource, TProjection> : ProjectionDefinition<TSource, TProjection>
    {
        private readonly BsonDocument _document;
        private readonly IBsonSerializer<TProjection> _projectionSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentProjectionDefinition{TSource, TProjection}"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <param name="projectionSerializer">The projection serializer.</param>
        public BsonDocumentProjectionDefinition(BsonDocument document, IBsonSerializer<TProjection> projectionSerializer = null)
        {
            _document = Ensure.IsNotNull(document, nameof(document));
            _projectionSerializer = projectionSerializer;
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        public BsonDocument Document
        {
            get { return _document; }
        }

        /// <summary>
        /// Gets the projection serializer.
        /// </summary>
        public IBsonSerializer<TProjection> ProjectionSerializer
        {
            get { return _projectionSerializer; }
        }

        /// <inheritdoc />
        public override RenderedProjectionDefinition<TProjection> Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return new RenderedProjectionDefinition<TProjection>(
                _document,
                _projectionSerializer ?? (sourceSerializer as IBsonSerializer<TProjection>) ?? serializerRegistry.GetSerializer<TProjection>());
        }
    }

    /// <summary>
    /// A find <see cref="Expression" /> based projection.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TProjection">The type of the projection.</typeparam>
    public sealed class FindExpressionProjectionDefinition<TSource, TProjection> : ProjectionDefinition<TSource, TProjection>
    {
        private readonly Expression<Func<TSource, TProjection>> _expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="FindExpressionProjectionDefinition{TSource, TProjection}" /> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public FindExpressionProjectionDefinition(Expression<Func<TSource, TProjection>> expression)
        {
            _expression = Ensure.IsNotNull(expression, nameof(expression));
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public Expression<Func<TSource, TProjection>> Expression
        {
            get { return _expression; }
        }

        /// <inheritdoc />
        public override RenderedProjectionDefinition<TProjection> Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return FindProjectionTranslator.Translate<TSource, TProjection>(_expression, sourceSerializer, serializerRegistry);
        }
    }

    /// <summary>
    /// A JSON <see cref="String" /> based projection whose projection type is not yet known.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public sealed class JsonProjectionDefinition<TSource> : ProjectionDefinition<TSource>
    {
        private readonly string _json;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonProjectionDefinition{TSource}"/> class.
        /// </summary>
        /// <param name="json">The json.</param>
        public JsonProjectionDefinition(string json)
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
        public override BsonDocument Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return BsonDocument.Parse(_json);
        }
    }

    /// <summary>
    /// A JSON <see cref="String" /> based projection.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TProjection">The type of the projection.</typeparam>
    public sealed class JsonProjectionDefinition<TSource, TProjection> : ProjectionDefinition<TSource, TProjection>
    {
        private readonly string _json;
        private readonly IBsonSerializer<TProjection> _projectionSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentSortDefinition{TDocument}" /> class.
        /// </summary>
        /// <param name="json">The json.</param>
        /// <param name="projectionSerializer">The projection serializer.</param>
        public JsonProjectionDefinition(string json, IBsonSerializer<TProjection> projectionSerializer = null)
        {
            _json = Ensure.IsNotNullOrEmpty(json, nameof(json));
            _projectionSerializer = projectionSerializer;
        }

        /// <summary>
        /// Gets the json.
        /// </summary>
        public string Json
        {
            get { return _json; }
        }

        /// <summary>
        /// Gets the projection serializer.
        /// </summary>
        public IBsonSerializer<TProjection> ProjectionSerializer
        {
            get { return _projectionSerializer; }
        }

        /// <inheritdoc />
        public override RenderedProjectionDefinition<TProjection> Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return new RenderedProjectionDefinition<TProjection>(
                BsonDocument.Parse(_json),
                _projectionSerializer ?? (sourceSerializer as IBsonSerializer<TProjection>) ?? serializerRegistry.GetSerializer<TProjection>());
        }
    }

    /// <summary>
    /// An <see cref="Object"/> based projection whose projection type is not yet known.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    public sealed class ObjectProjectionDefinition<TSource> : ProjectionDefinition<TSource>
    {
        private readonly object _obj;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectProjectionDefinition{TSource}"/> class.
        /// </summary>
        /// <param name="obj">The object.</param>
        public ObjectProjectionDefinition(object obj)
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
        public override BsonDocument Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var serializer = serializerRegistry.GetSerializer(_obj.GetType());
            return new BsonDocumentWrapper(_obj, serializer);
        }
    }

    /// <summary>
    /// An <see cref="Object"/> based projection.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TProjection">The type of the projection.</typeparam>
    public sealed class ObjectProjectionDefinition<TSource, TProjection> : ProjectionDefinition<TSource, TProjection>
    {
        private readonly object _obj;
        private readonly IBsonSerializer<TProjection> _projectionSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectProjectionDefinition{TSource, TProjection}" /> class.
        /// </summary>
        /// <param name="obj">The object.</param>
        /// <param name="projectionSerializer">The projection serializer.</param>
        public ObjectProjectionDefinition(object obj, IBsonSerializer<TProjection> projectionSerializer = null)
        {
            _obj = Ensure.IsNotNull(obj, nameof(obj));
            _projectionSerializer = projectionSerializer;
        }

        /// <summary>
        /// Gets the object.
        /// </summary>
        public object Object
        {
            get { return _obj; }
        }

        /// <summary>
        /// Gets the projection serializer.
        /// </summary>
        public IBsonSerializer<TProjection> ProjectionSerializer
        {
            get { return _projectionSerializer; }
        }

        /// <inheritdoc />
        public override RenderedProjectionDefinition<TProjection> Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var serializer = serializerRegistry.GetSerializer(_obj.GetType());
            return new RenderedProjectionDefinition<TProjection>(
                new BsonDocumentWrapper(_obj, serializer),
                _projectionSerializer ?? (sourceSerializer as IBsonSerializer<TProjection>) ?? serializerRegistry.GetSerializer<TProjection>());
        }
    }

    internal sealed class KnownResultTypeProjectionDefinitionAdapter<TSource, TProjection> : ProjectionDefinition<TSource, TProjection>
    {
        private readonly ProjectionDefinition<TSource> _projection;
        private readonly IBsonSerializer<TProjection> _projectionSerializer;

        public KnownResultTypeProjectionDefinitionAdapter(ProjectionDefinition<TSource> projection, IBsonSerializer<TProjection> projectionSerializer = null)
        {
            _projection = Ensure.IsNotNull(projection, nameof(projection));
            _projectionSerializer = projectionSerializer;
        }

        public ProjectionDefinition<TSource> Projection
        {
            get { return _projection; }
        }

        public IBsonSerializer<TProjection> ResultSerializer
        {
            get { return _projectionSerializer; }
        }

        public override RenderedProjectionDefinition<TProjection> Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            var document = _projection.Render(sourceSerializer, serializerRegistry);
            return new RenderedProjectionDefinition<TProjection>(
                document,
                _projectionSerializer ?? (sourceSerializer as IBsonSerializer<TProjection>) ?? serializerRegistry.GetSerializer<TProjection>());
        }
    }

    /// <summary>
    /// A client side only projection that is implemented solely by deserializing using a different serializer.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TProjection">The type of the projection.</typeparam>
    public sealed class ClientSideDeserializationProjectionDefinition<TSource, TProjection> : ProjectionDefinition<TSource, TProjection>
    {
        private readonly IBsonSerializer<TProjection> _projectionSerializer;

        /// <summary>
        /// Initializes a new instance of the <see cref="ClientSideDeserializationProjectionDefinition{TSource, TProjection}"/> class.
        /// </summary>
        /// <param name="projectionSerializer">The projection serializer.</param>
        public ClientSideDeserializationProjectionDefinition(IBsonSerializer<TProjection> projectionSerializer = null)
        {
            _projectionSerializer = projectionSerializer;
        }

        /// <summary>
        /// Gets the result serializer.
        /// </summary>
        /// <value>
        /// The result serializer.
        /// </value>
        public IBsonSerializer<TProjection> ResultSerializer
        {
            get { return _projectionSerializer; }
        }

        /// <inheritdoc/>
        public override RenderedProjectionDefinition<TProjection> Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return new RenderedProjectionDefinition<TProjection>(
                null,
                _projectionSerializer ?? (sourceSerializer as IBsonSerializer<TProjection>) ?? serializerRegistry.GetSerializer<TProjection>());
        }
    }
}
