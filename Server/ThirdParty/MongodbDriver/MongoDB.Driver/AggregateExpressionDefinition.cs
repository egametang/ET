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

using System;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Linq.Translators;

namespace MongoDB.Driver
{
    /// <summary>
    /// An aggregation expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class AggregateExpressionDefinition<TSource, TResult>
    {
        #region static
        // public implicit conversions
        /// <summary>
        /// Performs an implicit conversion from <see cref="BsonValue"/> to <see cref="AggregateExpressionDefinition{TSource, TResult}"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator AggregateExpressionDefinition<TSource, TResult>(BsonValue expression)
        {
            Ensure.IsNotNull(expression, nameof(expression));
            return new BsonValueAggregateExpressionDefinition<TSource, TResult>(expression);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="AggregateExpressionDefinition{TSource, TResult}"/>.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator AggregateExpressionDefinition<TSource, TResult>(string expression)
        {
            Ensure.IsNotNullOrEmpty(expression, nameof(expression));
            if (expression[0] == '{')
            {
                return new BsonValueAggregateExpressionDefinition<TSource, TResult>(BsonDocument.Parse(expression));
            }
            else
            {
                return new BsonValueAggregateExpressionDefinition<TSource, TResult>(new BsonString(expression));
            }
        }
        #endregion

        /// <summary>
        /// Renders the aggregation expression.
        /// </summary>
        /// <param name="sourceSerializer">The source serializer.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>The rendered aggregation expression.</returns>
        public abstract BsonValue Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry);
    }

    /// <summary>
    /// A <see cref="BsonValue"/> based aggregate expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="MongoDB.Driver.AggregateExpressionDefinition{TSource, TResult}" />
    public sealed class BsonValueAggregateExpressionDefinition<TSource, TResult> : AggregateExpressionDefinition<TSource, TResult>
    {
        // private fields
        private readonly BsonValue _expression;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonValueAggregateExpressionDefinition{TSource, TResult}"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        public BsonValueAggregateExpressionDefinition(BsonValue expression)
        {
            _expression = Ensure.IsNotNull(expression, nameof(expression));
        }

        // public methods        
        /// <inheritdoc/>
        public override BsonValue Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return _expression;
        }
    }

    /// <summary>
    /// A <see cref="BsonValue"/> based aggregate expression.
    /// </summary>
    /// <typeparam name="TSource">The type of the source.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <seealso cref="MongoDB.Driver.AggregateExpressionDefinition{TSource, TResult}" />
    public sealed class ExpressionAggregateExpressionDefinition<TSource, TResult> : AggregateExpressionDefinition<TSource, TResult>
    {
        // private fields
        private readonly Expression<Func<TSource, TResult>> _expression;
        private readonly ExpressionTranslationOptions _translationOptions;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionAggregateExpressionDefinition{TSource, TResult}" /> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <param name="translationOptions">The translation options.</param>
        public ExpressionAggregateExpressionDefinition(Expression<Func<TSource, TResult>> expression, ExpressionTranslationOptions translationOptions)
        {
            _expression = Ensure.IsNotNull(expression, nameof(expression));
            _translationOptions = translationOptions; // can be null
        }

        // public methods
        /// <inheritdoc/>
        public override BsonValue Render(IBsonSerializer<TSource> sourceSerializer, IBsonSerializerRegistry serializerRegistry)
        {
            return AggregateExpressionTranslator.Translate(_expression, sourceSerializer, serializerRegistry, _translationOptions);
        }
    }
}
