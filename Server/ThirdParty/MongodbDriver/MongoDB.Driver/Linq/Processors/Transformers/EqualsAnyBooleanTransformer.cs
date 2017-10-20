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

namespace MongoDB.Driver.Linq.Processors.Transformers
{
    internal sealed class EqualsAnyBooleanTransformer : IExpressionTransformer<BinaryExpression>
    {
        private readonly ExpressionType[] _supportedNodeTypes = new[]
        {
            ExpressionType.Equal,
            ExpressionType.NotEqual
        };

        public ExpressionType[] SupportedNodeTypes
        {
            get { return _supportedNodeTypes; }
        }

        public Expression Transform(BinaryExpression node)
        {
            var call = node.Left as MethodCallExpression;
            if (call == null || !ExpressionHelper.IsLinqMethod(call, "Any"))
            {
                return node;
            }

            var constant = node.Right as ConstantExpression;
            if (constant == null)
            {
                return node;
            }

            var value = (bool)constant.Value;

            if (node.NodeType == ExpressionType.NotEqual)
            {
                value = !value;
            }

            return value ?
                (Expression)call :
                Expression.Not(call);
        }
    }
}
