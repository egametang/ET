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
    /// <remarks>
    /// VB uses a method for string comparisons, so we'll convert this into a BinaryExpression.
    /// </remarks>
    internal sealed class VBCompareStringTransformer : IExpressionTransformer<BinaryExpression>
    {
        private readonly ExpressionType[] _supportedNodeTypes = new[]
        {
            ExpressionType.Equal,
            ExpressionType.GreaterThan,
            ExpressionType.GreaterThanOrEqual,
            ExpressionType.LessThan,
            ExpressionType.LessThanOrEqual,
            ExpressionType.NotEqual
        };

        public ExpressionType[] SupportedNodeTypes
        {
            get { return _supportedNodeTypes; }
        }

        public Expression Transform(BinaryExpression node)
        {
            if (node.Left.NodeType != ExpressionType.Call || node.Right.NodeType != ExpressionType.Constant)
            {
                return node;
            }

            var methodCall = (MethodCallExpression)node.Left;
            if (methodCall.Method.DeclaringType.FullName != "Microsoft.VisualBasic.CompilerServices.Operators" ||
                methodCall.Method.Name != "CompareString")
            {
                return node;
            }

            var valueConstant = (ConstantExpression)node.Right;
            if (valueConstant.Type != typeof(int) || (int)valueConstant.Value != 0)
            {
                return node;
            }

            var caseSensitiveConstant = (ConstantExpression)methodCall.Arguments[2];

            var comparand = (int)valueConstant.Value;
            bool caseSensitive = (bool)caseSensitiveConstant.Value;

            if (comparand != 0 || !caseSensitive)
            {
                return node;
            }

            return Expression.MakeBinary(
                node.NodeType,
                methodCall.Arguments[0],
                methodCall.Arguments[1]);
        }
    }
}
