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
    /// MongoDB only supports constants on the RHS for certain expressions, so we'll move them around
    /// to make it easier to generate MongoDB syntax.
    /// </remarks>
    internal sealed class ConstantOnRightTransformer : IExpressionTransformer<BinaryExpression>
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
            var left = node.Left;
            var right = node.Right;
            var nodeType = node.NodeType;

            if (RemoveUnnecessaries(left).NodeType == ExpressionType.Constant)
            {
                right = node.Left;
                left = node.Right;

                switch (nodeType)
                {
                    case ExpressionType.LessThan: nodeType = ExpressionType.GreaterThan; break;
                    case ExpressionType.LessThanOrEqual: nodeType = ExpressionType.GreaterThanOrEqual; break;
                    case ExpressionType.GreaterThan: nodeType = ExpressionType.LessThan; break;
                    case ExpressionType.GreaterThanOrEqual: nodeType = ExpressionType.LessThanOrEqual; break;
                }
            }

            if (left != node.Left || right != node.Right || nodeType != node.NodeType)
            {
                return Expression.MakeBinary(
                    nodeType,
                    left,
                    right,
                    node.IsLiftedToNull,
                    node.Method,
                    node.Conversion);
            }

            return node;
        }

        private Expression RemoveUnnecessaries(Expression node)
        {
            while (node.NodeType == ExpressionType.Convert ||
                node.NodeType == ExpressionType.ConvertChecked ||
                node.NodeType == ExpressionType.Quote)
            {
                node = ((UnaryExpression)node).Operand;
            }

            return node;
        }
    }
}
