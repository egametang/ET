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

namespace MongoDB.Driver.Linq.Processors.Transformers
{
    internal sealed class FirstLastNormalizingTransformer : IExpressionTransformer<MemberExpression>
    {
        private readonly ExpressionType[] _supportedNodeTypes = new[]
        {
            ExpressionType.MemberAccess
        };

        public ExpressionType[] SupportedNodeTypes
        {
            get { return _supportedNodeTypes; }
        }

        public Expression Transform(MemberExpression node)
        {
            var members = new Stack<MemberInfo>();
            Expression currentNode = node;
            while (currentNode != null && currentNode.NodeType == ExpressionType.MemberAccess)
            {
                var mex = (MemberExpression)currentNode;
                members.Push(mex.Member);
                currentNode = mex.Expression;
            }

            // we are going to rewrite g.Last().Member to g.Select(x => x.Member).Last()
            var call = currentNode as MethodCallExpression;
            if (call != null && IsAggregateMethod(call.Method.Name) && call.Arguments.Count == 1)
            {
                var source = call.Arguments[0];
                var sourceType = call.Method.GetGenericArguments()[0];
                var parameter = Expression.Parameter(sourceType, "x");

                Expression lambdaBody = parameter;
                while (members.Count > 0)
                {
                    var currentMember = members.Pop();
                    lambdaBody = Expression.MakeMemberAccess(lambdaBody, currentMember);
                }

                var select = Expression.Call(
                    typeof(Enumerable),
                    "Select",
                    new[] { sourceType, lambdaBody.Type },
                    source,
                    Expression.Lambda(
                        typeof(Func<,>).MakeGenericType(sourceType, lambdaBody.Type),
                        lambdaBody,
                        parameter));

                return Expression.Call(
                    typeof(Enumerable),
                    call.Method.Name,
                    new[] { lambdaBody.Type },
                    select);
            }

            return node;
        }

        private bool IsAggregateMethod(string methodName)
        {
            return methodName == "First" || methodName == "Last";
        }
    }
}
