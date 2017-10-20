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

using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Driver.Linq.Expressions;
using MongoDB.Driver.Linq.Expressions.ResultOperators;
using MongoDB.Driver.Support;

namespace MongoDB.Driver.Linq.Processors.EmbeddedPipeline.MethodCallBinders
{
    internal sealed class AggregateBinder : IMethodCallBinder<EmbeddedPipelineBindingContext>
    {
        public static IEnumerable<MethodInfo> GetSupportedMethods()
        {
            yield return MethodHelper.GetMethodDefinition(() => Enumerable.Aggregate<object>(null, null));
            yield return MethodHelper.GetMethodDefinition(() => Enumerable.Aggregate<object, object>(null, null, null));
            yield return MethodHelper.GetMethodDefinition(() => Enumerable.Aggregate<object, object, object>(null, null, null, null));
            yield return MethodHelper.GetMethodDefinition(() => Queryable.Aggregate<object>(null, null));
            yield return MethodHelper.GetMethodDefinition(() => Queryable.Aggregate<object, object>(null, null, null));
            yield return MethodHelper.GetMethodDefinition(() => Queryable.Aggregate<object, object, object>(null, null, null, null));
        }

        public Expression Bind(PipelineExpression pipeline, EmbeddedPipelineBindingContext bindingContext, MethodCallExpression node, IEnumerable<Expression> arguments)
        {
            var sourceSerializer = pipeline.Projector.Serializer;
            var accumulatorSerializer = sourceSerializer;
            Expression seed;
            LambdaExpression lambda;
            var argumentCount = arguments.Count();
            if (argumentCount == 1)
            {
                lambda = ExpressionHelper.GetLambda(arguments.Single());
                seed = Expression.Constant(accumulatorSerializer.ValueType.GetDefaultValue());
            }
            else
            {
                seed = arguments.First();
                accumulatorSerializer = bindingContext.GetSerializer(seed.Type, seed);
                lambda = ExpressionHelper.GetLambda(arguments.ElementAt(1));
            }

            if (seed.NodeType == ExpressionType.Constant)
            {
                seed = new SerializedConstantExpression(((ConstantExpression)seed).Value, accumulatorSerializer);
            }

            var valueField = new FieldExpression("$value", accumulatorSerializer);
            var thisField = new FieldExpression("$this", sourceSerializer);

            bindingContext.AddExpressionMapping(lambda.Parameters[0], valueField);
            bindingContext.AddExpressionMapping(lambda.Parameters[1], thisField);

            var reducer = bindingContext.Bind(lambda.Body);
            var serializer = bindingContext.GetSerializer(reducer.Type, reducer);

            Expression finalizer = null;
            string itemName = null;
            if (argumentCount == 3)
            {
                lambda = ExpressionHelper.GetLambda(arguments.Last());
                itemName = lambda.Parameters[0].Name;
                var variable = new FieldExpression("$" + itemName, serializer);
                bindingContext.AddExpressionMapping(lambda.Parameters[0], variable);
                finalizer = bindingContext.Bind(lambda.Body);
                serializer = bindingContext.GetSerializer(finalizer.Type, finalizer);
            }

            return new PipelineExpression(
                pipeline.Source,
                new DocumentExpression(serializer),
                new AggregateResultOperator(seed, reducer, finalizer, itemName, serializer));
        }
    }
}
