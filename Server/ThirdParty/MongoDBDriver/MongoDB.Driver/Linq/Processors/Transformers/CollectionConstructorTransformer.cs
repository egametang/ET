/* Copyright 2015-present MongoDB Inc.
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

namespace MongoDB.Driver.Linq.Processors.Transformers
{
    internal sealed class CollectionConstructorTransformer : IExpressionTransformer<NewExpression>
    {
        private readonly ExpressionType[] _supportedNodeTypes = new[]
        {
            ExpressionType.New
        };

        public ExpressionType[] SupportedNodeTypes
        {
            get { return _supportedNodeTypes; }
        }

        public Expression Transform(NewExpression node)
        {
            var isGenericType = node.Type.GetTypeInfo().IsGenericType;

            if (isGenericType &&
                node.Type.GetGenericTypeDefinition() == typeof(HashSet<>) &&
                node.Arguments.Count == 1)
            {
                return Expression.Call(
                    typeof(MongoEnumerable),
                    "ToHashSet",
                    node.Type.GetTypeInfo().GetGenericArguments(),
                    node.Arguments.ToArray());
            }

            if (isGenericType &&
                node.Type.GetGenericTypeDefinition() == typeof(List<>) &&
                node.Arguments.Count == 1)
            {
                return Expression.Call(
                    typeof(Enumerable),
                    "ToList",
                    node.Type.GetTypeInfo().GetGenericArguments(),
                    node.Arguments.ToArray());
            }

            return node;
        }
    }
}
