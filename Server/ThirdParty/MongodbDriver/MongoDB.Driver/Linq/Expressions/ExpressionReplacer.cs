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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq.Expressions
{
    internal sealed class ExpressionReplacer : ExtensionExpressionVisitor
    {
        public static Expression Replace(Expression node, Expression original, Expression replacement)
        {
            var visitor = new ExpressionReplacer(original, replacement);
            return visitor.Visit(node);
        }

        private readonly Expression _original;
        private readonly Expression _replacement;

        private ExpressionReplacer(Expression original, Expression replacement)
        {
            _original = Ensure.IsNotNull(original, nameof(original));
            _replacement = Ensure.IsNotNull(replacement, nameof(replacement));
        }

        public override Expression Visit(Expression node)
        {
            if (node == _original)
            {
                return _replacement;
            }

            return base.Visit(node);
        }
    }
}
