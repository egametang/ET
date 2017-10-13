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
* 
*/

using System;
using System.Linq.Expressions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Processors;

namespace MongoDB.Driver.Linq.Translators
{
    internal static class AggregateExpressionTranslator
    {
        public static BsonValue Translate<TSource, TResult>(
            Expression<Func<TSource, TResult>> expression,
            IBsonSerializer<TSource> sourceSerializer,
            IBsonSerializerRegistry serializerRegistry,
            ExpressionTranslationOptions translationOptions)
        {
            var bindingContext = new PipelineBindingContext(serializerRegistry);
            var boundExpression = BindResult(bindingContext, expression, sourceSerializer);
            return AggregateLanguageTranslator.Translate(boundExpression, translationOptions);
        }

        private static Expression BindResult<TSource,TResult>(
            PipelineBindingContext bindingContext,
            Expression<Func<TSource, TResult>> expression,
            IBsonSerializer<TSource> sourceSerializer)
        {
            var parameterExpression = new DocumentExpression(sourceSerializer);
            bindingContext.AddExpressionMapping(expression.Parameters[0], parameterExpression);
            var node = PartialEvaluator.Evaluate(expression.Body);
            node = Transformer.Transform(node);
            node = bindingContext.Bind(node);

            var resultSerializer = bindingContext.GetSerializer(node.Type, node);
            return new AggregateExpressionExpression(node, resultSerializer);
        }
    }
}
