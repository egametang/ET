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
using MongoDB.Driver.Linq;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors;
using MongoDB.Driver.Linq.Translators;

namespace MongoDB.Driver
{
    /// <summary>
    /// Base class for filters.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public abstract class FilterDefinition<TDocument>
    {
        private static readonly FilterDefinition<TDocument> __empty = new EmptyFilterDefinition<TDocument>();

        /// <summary>
        /// Gets an empty filter. An empty filter matches everything.
        /// </summary>
        public static FilterDefinition<TDocument> Empty
        {
            get { return __empty; }
        }

        /// <summary>
        /// Renders the filter to a <see cref="BsonDocument"/>.
        /// </summary>
        /// <param name="documentSerializer">The document serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>A <see cref="BsonDocument"/>.</returns>
        public abstract BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry);

        /// <summary>
        /// Performs an implicit conversion from <see cref="BsonDocument"/> to <see cref="FilterDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="document">The document.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator FilterDefinition<TDocument>(BsonDocument document)
        {
            if (document == null)
            {
                return null;
            }

            return new BsonDocumentFilterDefinition<TDocument>(document);
        }

        /// <summary>
        /// Performs an implicit conversion from a predicate expression to <see cref="FilterDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="predicate">The predicate.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator FilterDefinition<TDocument>(Expression<Func<TDocument, bool>> predicate)
        {
            if (predicate == null)
            {
                return null;
            }

            return new ExpressionFilterDefinition<TDocument>(predicate);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="FilterDefinition{TDocument}"/>.
        /// </summary>
        /// <param name="json">The JSON string.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator FilterDefinition<TDocument>(string json)
        {
            if (json == null)
            {
                return null;
            }

            return new JsonFilterDefinition<TDocument>(json);
        }

        /// <summary>
        /// Implements the operator &amp;.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static FilterDefinition<TDocument> operator &(FilterDefinition<TDocument> lhs, FilterDefinition<TDocument> rhs)
        {
            return new AndFilterDefinition<TDocument>(new[] { lhs, rhs });
        }

        /// <summary>
        /// Implements the operator |.
        /// </summary>
        /// <param name="lhs">The LHS.</param>
        /// <param name="rhs">The RHS.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static FilterDefinition<TDocument> operator |(FilterDefinition<TDocument> lhs, FilterDefinition<TDocument> rhs)
        {
            return new OrFilterDefinition<TDocument>(new[] { lhs, rhs });
        }

        /// <summary>
        /// Implements the operator !.
        /// </summary>
        /// <param name="op">The op.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static FilterDefinition<TDocument> operator !(FilterDefinition<TDocument> op)
        {
            return new NotFilterDefinition<TDocument>(op);
        }
    }

    /// <summary>
    /// A <see cref="BsonDocument"/> based filter.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class BsonDocumentFilterDefinition<TDocument> : FilterDefinition<TDocument>
    {
        private readonly BsonDocument _document;

        /// <summary>
        /// Initializes a new instance of the <see cref="BsonDocumentFilterDefinition{TDocument}"/> class.
        /// </summary>
        /// <param name="document">The document.</param>
        public BsonDocumentFilterDefinition(BsonDocument document)
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

    internal sealed class EmptyFilterDefinition<TDocument> : FilterDefinition<TDocument>
    {
        /// <inheritdoc />
        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return new BsonDocument();
        }
    }

    /// <summary>
    /// An <see cref="Expression"/> based filter.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class ExpressionFilterDefinition<TDocument> : FilterDefinition<TDocument>
    {
        private readonly Expression<Func<TDocument, bool>> _expression;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionFilterDefinition{TDocument}"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public ExpressionFilterDefinition(Expression<Func<TDocument, bool>> expression)
        {
            _expression = Ensure.IsNotNull(expression, nameof(expression));
        }

        /// <summary>
        /// Gets the expression.
        /// </summary>
        public Expression<Func<TDocument, bool>> Expression
        {
            get { return _expression; }
        }

        /// <inheritdoc />
        public override BsonDocument Render(IBsonSerializer<TDocument> documentSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return PredicateTranslator.Translate<TDocument>(_expression, documentSerializer, serializerRegistry);
        }
    }

    /// <summary>
    /// A JSON <see cref="String" /> based filter.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class JsonFilterDefinition<TDocument> : FilterDefinition<TDocument>
    {
        private readonly string _json;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonFilterDefinition{TDocument}" /> class.
        /// </summary>
        /// <param name="json">The json.</param>
        public JsonFilterDefinition(string json)
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
    /// An <see cref="Object" /> based filter.
    /// </summary>
    /// <typeparam name="TDocument">The type of the document.</typeparam>
    public sealed class ObjectFilterDefinition<TDocument> : FilterDefinition<TDocument>
    {
        private readonly object _obj;

        /// <summary>
        /// Initializes a new instance of the <see cref="ObjectFilterDefinition{TDocument}"/> class.
        /// </summary>
        /// <param name="obj">The object.</param>
        public ObjectFilterDefinition(object obj)
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
