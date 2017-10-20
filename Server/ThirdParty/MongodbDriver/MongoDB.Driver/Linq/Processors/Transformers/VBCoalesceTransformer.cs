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
    /// <remarks>
    /// VB creates coalescing operations when dealing with nullable value comparisons, so we try and make this look like C#
    /// </remarks>
    internal sealed class VBCoalesceTransformer : IExpressionTransformer<BinaryExpression>
    {
        private readonly ExpressionType[] _supportedNodeTypes = new[]
        {
            ExpressionType.Coalesce
        };

        public ExpressionType[] SupportedNodeTypes
        {
            get { return _supportedNodeTypes; }
        }

        public Expression Transform(BinaryExpression node)
        {
            var right = node.Right as ConstantExpression;
            if (node.Left.NodeType != ExpressionType.Equal ||
                !node.Left.Type.IsNullable() ||
                right == null ||
                right.Type != typeof(bool) ||
                (bool)right.Value)
            {
                return node;
            }

            return Expression.MakeBinary(
                ExpressionType.Equal,
                node.Left,
                node.Right,
                false,
                null);
        }
    }
}
