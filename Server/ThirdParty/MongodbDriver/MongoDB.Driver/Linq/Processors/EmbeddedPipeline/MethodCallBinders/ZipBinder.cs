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

namespace MongoDB.Driver.Linq.Processors.EmbeddedPipeline.MethodCallBinders
{
    internal sealed class ZipBinder : IMethodCallBinder<EmbeddedPipelineBindingContext>
    {
        public static IEnumerable<MethodInfo> GetSupportedMethods()
        {
            yield return MethodHelper.GetMethodDefinition(() => Enumerable.Zip<object, object, object>(null, null, null));
            yield return MethodHelper.GetMethodDefinition(() => Queryable.Zip<object, object, object>(null, null, null));
        }

        public Expression Bind(PipelineExpression pipeline, EmbeddedPipelineBindingContext bindingContext, MethodCallExpression node, IEnumerable<Expression> arguments)
        {
            var source = new ZipExpression(
                node.Type,
                pipeline.Source,
                arguments.First());

            var lambda = ExpressionHelper.GetLambda(arguments.Last());
            var input = new DocumentExpression(bindingContext.GetSerializer(typeof(object[]), null));

            var itemA = new ArrayIndexExpression(
                input,
                Expression.Constant(0),
                bindingContext.GetSerializer(lambda.Parameters[0].Type, source.Source));
            var itemB = new ArrayIndexExpression(
                input,
                Expression.Constant(1),
                bindingContext.GetSerializer(lambda.Parameters[1].Type, source.Other));

            bindingContext.AddExpressionMapping(lambda.Parameters[0], itemA);
            bindingContext.AddExpressionMapping(lambda.Parameters[1], itemB);

            var selector = bindingContext.Bind(lambda.Body);

            var serializer = bindingContext.GetSerializer(selector.Type, selector);

            return new PipelineExpression(
                new SelectExpression(
                    source,
                    lambda.Parameters[0].Name + "_" + lambda.Parameters[1].Name,
                    selector),
                new DocumentExpression(serializer));
        }
    }
}
