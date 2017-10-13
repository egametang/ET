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
    /// VB introduces a Convert on the LHS with a Nothing comparison, so we make it look like 
    /// C# which does not have one with a comparison to null.
    /// </remarks>
    internal sealed class VBNothingConversionRemovalTransformer : IExpressionTransformer<BinaryExpression>
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
            if (node.Left.NodeType != ExpressionType.Convert ||
                node.Right.NodeType != ExpressionType.Constant)
            {
                return node;
            }
            var left = (UnaryExpression)node.Left;
            var right = (ConstantExpression)node.Right;

            if (left.Type != typeof(object) || right.Value == null)
            {
                return node;
            }

            return Expression.MakeBinary(
                node.NodeType,
                left.Operand,
                node.Right);
        }
    }
}
