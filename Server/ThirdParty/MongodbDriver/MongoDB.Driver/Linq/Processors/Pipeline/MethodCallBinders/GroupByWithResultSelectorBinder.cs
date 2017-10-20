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
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors.Pipeline.MethodCallBinders
{
    internal sealed class GroupByWithResultSelectorBinder : IMethodCallBinder<PipelineBindingContext>
    {
        public static IEnumerable<MethodInfo> GetSupportedMethods()
        {
            yield return MethodHelper.GetMethodDefinition(() => Enumerable.GroupBy(null, null, (Func<object, IEnumerable<object>, object>)null));
            yield return MethodHelper.GetMethodDefinition(() => Queryable.GroupBy(null, null, (Expression<Func<object, IEnumerable<object>, object>>)null));
        }

        public Expression Bind(PipelineExpression pipeline, PipelineBindingContext bindingContext, MethodCallExpression node, IEnumerable<Expression> arguments)
        {
            var keySelector = BindKey(pipeline, bindingContext, arguments.First());
            var selector = BindSelector(pipeline, bindingContext, keySelector, arguments.Last());
            var serializer = bindingContext.GetSerializer(selector.Type, selector);

            return new PipelineExpression(
                new GroupByWithResultSelectorExpression(
                    pipeline.Source,
                    selector),
                new DocumentExpression(serializer));
        }

        private Expression BindKey(PipelineExpression pipeline, PipelineBindingContext bindingContext, Expression node)
        {
            var lambda = ExpressionHelper.GetLambda(node);
            bindingContext.AddExpressionMapping(lambda.Parameters[0], pipeline.Projector);
            return bindingContext.Bind(lambda.Body);
        }

        private Expression BindSelector(PipelineExpression pipeline, PipelineBindingContext bindingContext, Expression keySelector, Expression node)
        {
            var lambda = ExpressionHelper.GetLambda(node);

            var serializer = bindingContext.GetSerializer(pipeline.Projector.Type, pipeline.Projector);
            var sequenceSerializer = (IBsonSerializer)Activator.CreateInstance(
                typeof(ArraySerializer<>).MakeGenericType(pipeline.Projector.Type),
                new object[] { serializer });
            var sequence = new DocumentExpression(sequenceSerializer);

            var keySerializer = bindingContext.GetSerializer(keySelector.Type, keySelector);

            Guid correlationId = Guid.NewGuid();
            bindingContext.AddCorrelatingId(sequence, correlationId);
            bindingContext.AddExpressionMapping(lambda.Parameters[0], new GroupingKeyExpression(keySelector, keySerializer));
            bindingContext.AddExpressionMapping(lambda.Parameters[1], sequence);
            var bound = bindingContext.Bind(lambda.Body);
            return CorrelatedAccumulatorRemover.Remove(bound, correlationId);
        }
    }
}
