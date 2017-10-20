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
    /// VB creates an IsNothing comparison using a method call. We'll translate this to a simple
    /// null comparison.
    /// </remarks>
    internal sealed class VBInformationIsNothingTransformer : IExpressionTransformer<MethodCallExpression>
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
            if (node.Method.DeclaringType.FullName != "Microsoft.VisualBasic.Information" ||
                node.Method.Name != "IsNothing")
            {
                return node;
            }

            return Expression.Equal(node.Arguments[0], Expression.Constant(null));
        }
    }
}
