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
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors.Transformers
{
    internal sealed class SelectSelectCombiningTransformer : IExpressionTransformer<MethodCallExpression>
    {
        private readonly ExpressionType[] _supportedNodeTypes = new[]
        {
            ExpressionType.Call
        };

        public ExpressionType[] SupportedNodeTypes
        {
            get { return _supportedNodeTypes; }
        }

        public Expression Transform(MethodCallExpression node)
        {
            if (!ExpressionHelper.IsLinqMethod(node, "Select") ||
                !ExpressionHelper.IsLambda(node.Arguments[1], 1))
            {
                return node;
            }

            var call = node.Arguments[0] as MethodCallExpression;

            if (call == null ||
                !ExpressionHelper.IsLinqMethod(call, "Select") ||
                !ExpressionHelper.IsLambda(call.Arguments[1], 1))
            {
                return node;
            }

            var innerLambda = ExpressionHelper.GetLambda(call.Arguments[1]);
            var outerLambda = ExpressionHelper.GetLambda(node.Arguments[1]);

            var sourceType = innerLambda.Parameters[0].Type;
            var resultType = outerLambda.Body.Type;

            var innerSelector = innerLambda.Body;
            var outerSelector = outerLambda.Body;

            var selector = ExpressionReplacer.Replace(outerSelector, outerLambda.Parameters[0], innerSelector);

            return Expression.Call(
                typeof(Enumerable),
                "Select",
                new[] { sourceType, resultType },
                call.Arguments[0],
                Expression.Lambda(
                    typeof(Func<,>).MakeGenericType(sourceType, resultType),
                    selector,
                    innerLambda.Parameters[0]));
        }
    }
}
