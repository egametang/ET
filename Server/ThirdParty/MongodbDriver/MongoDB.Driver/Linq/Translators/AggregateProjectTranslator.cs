/* Copyright 2015 MongoDB Inc.
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
    internal static class AggregateProjectTranslator
    {
        public static RenderedProjectionDefinition<TResult> Translate<TDocument, TResult>(Expression<Func<TDocument, TResult>> projector, IBsonSerializer<TDocument> parameterSerializer, IBsonSerializerRegistry serializerRegistry, ExpressionTranslationOptions translationOptions)
        {
            var bindingContext = new PipelineBindingContext(serializerRegistry);
            var parameterExpression = new DocumentExpression(parameterSerializer);
            bindingContext.AddExpressionMapping(projector.Parameters[0], parameterExpression);

            var node = PartialEvaluator.Evaluate(projector.Body);
            node = Transformer.Transform(node);
            node = bindingContext.Bind(node);

            var projectionSerializer = bindingContext.GetSerializer(node.Type, node);
            var projection = TranslateProject(node, translationOptions);

            return new RenderedProjectionDefinition<TResult>(projection, (IBsonSerializer<TResult>)projectionSerializer);
        }

        public static BsonDocument TranslateProject(Expression expression, ExpressionTranslationOptions translationOptions)
        {
            var projection = (BsonDocument)AggregateLanguageTranslator.Translate(expression, translationOptions);
            if (!projection.Contains("_id"))
            {
                projection.Add("_id", 0);
            }

            return projection;
        }
    }
}
