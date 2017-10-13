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

using System.Linq.Expressions;
using MongoDB.Driver.Support;

namespace MongoDB.Driver.Linq.Processors.Transformers
{
    internal sealed class NullableTransformer : IExpressionTransformer<MemberExpression>
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
            if (!node.Member.DeclaringType.IsNullable())
            {
                return node;
            }

            if (node.Member.Name == "Value")
            {
                return Expression.Convert(node.Expression, node.Type);
            }

            if (node.Member.Name == "HasValue")
            {
                return Expression.NotEqual(node.Expression, Expression.Constant(null, node.Member.DeclaringType));
            }

            return node;
        }
    }
}
