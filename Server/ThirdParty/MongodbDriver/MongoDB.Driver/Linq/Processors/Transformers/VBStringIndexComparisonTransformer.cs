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
using System.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors.Transformers
{
    /// <remarks>
    /// VB creates string index expressions using character comparison whereas C# uses ascii value comparison
    /// we make VB's string index comparison look like C#.
    /// </remarks>
    internal sealed class VBStringIndexComparisonTransformer : IExpressionTransformer<BinaryExpression>
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
            if (node.Left.NodeType != ExpressionType.Call ||
                node.Right.NodeType != ExpressionType.Constant)
            {
                return node;
            }

            var methodCall = (MethodCallExpression)node.Left;
            var constant = (ConstantExpression)node.Right;

            if (methodCall.Method.DeclaringType != typeof(string) ||
                methodCall.Method.Name != "get_Chars" ||
                constant.Type != typeof(char))
            {
                return node;
            }

            return Expression.MakeBinary(
                node.NodeType,
                Expression.Convert(methodCall, typeof(int)),
                Expression.Constant(Convert.ToInt32((char)constant.Value)));
        }
    }
}
