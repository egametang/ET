/* Copyright 2015-2016 MongoDB Inc.
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
* 
*/

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors;

namespace MongoDB.Driver.Linq.Translators
{
    internal static class AggregateGroupTranslator
    {
        public static RenderedProjectionDefinition<TResult> Translate<TKey, TDocument, TResult>(Expression<Func<TDocument, TKey>> idProjector, Expression<Func<IGrouping<TKey, TDocument>, TResult>> groupProjector, IBsonSerializer<TDocument> parameterSerializer, IBsonSerializerRegistry serializerRegistry, ExpressionTranslationOptions translationOptions)
        {
            var bindingContext = new PipelineBindingContext(serializerRegistry);

            var keySelector = BindKeySelector(bindingContext, idProjector, parameterSerializer);
            var boundGroupExpression = BindGroup(bindingContext, groupProjector, parameterSerializer, keySelector);

            var projectionSerializer = bindingContext.GetSerializer(boundGroupExpression.Type, boundGroupExpression);
            var projection = AggregateLanguageTranslator.Translate(boundGroupExpression, translationOptions).AsBsonDocument;

            // must have an "_id" in a group document
            if (!projection.Contains("_id"))
            {
                var idProjection = AggregateLanguageTranslator.Translate(keySelector, translationOptions);
                projection.InsertAt(0, new BsonElement("_id", idProjection));
            }

            return new RenderedProjectionDefinition<TResult>(projection, (IBsonSerializer<TResult>)projectionSerializer);
        }

        private static Expression BindKeySelector<TKey, TDocument>(PipelineBindingContext bindingContext, Expression<Func<TDocument, TKey>> keySelector, IBsonSerializer<TDocument> parameterSerializer)
        {
            var parameterExpression = new DocumentExpression(parameterSerializer);
            bindingContext.AddExpressionMapping(keySelector.Parameters[0], parameterExpression);
            var node = PartialEvaluator.Evaluate(keySelector.Body);
            node = Transformer.Transform(node);
            node = bindingContext.Bind(node);

            var keySerializer = bindingContext.GetSerializer(node.Type, node);
            return new GroupingKeyExpression(node, keySerializer);
        }

        private static Expression BindGroup<TKey, TDocument, TResult>(PipelineBindingContext bindingContext, Expression<Func<IGrouping<TKey, TDocument>, TResult>> groupProjector, IBsonSerializer<TDocument> parameterSerializer, Expression keySelector)
        {
            var groupSerializer = new ArraySerializer<TDocument>(parameterSerializer);
            var groupExpression = new DocumentExpression(groupSerializer);

            var correlationId = Guid.NewGuid();
            bindingContext.AddCorrelatingId(groupExpression, correlationId);
            bindingContext.AddExpressionMapping(groupProjector.Parameters[0], groupExpression);
            bindingContext.AddMemberMapping(typeof(IGrouping<TKey, TDocument>).GetTypeInfo().GetProperty("Key"), keySelector);

            var node = PartialEvaluator.Evaluate(groupProjector.Body);
            node = Transformer.Transform(node);
            node = bindingContext.Bind(node);

            return CorrelatedAccumulatorRemover.Remove(node, correlationId);
        }
    }
}
