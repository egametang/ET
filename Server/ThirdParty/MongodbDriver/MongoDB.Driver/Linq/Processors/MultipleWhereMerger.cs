/* Copyright 2015-2016 MongoDB Inc.
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
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors
{
    internal class MultipleWhereMerger : ExtensionExpressionVisitor
    {
        public static Expression Merge(Expression node)
        {
            var visitor = new MultipleWhereMerger();
            return visitor.Visit(node);
        }

        protected internal override Expression VisitWhere(WhereExpression node)
        {
            var sourceWhere = node.Source as WhereExpression;
            if (sourceWhere != null)
            {
                var source = sourceWhere.Source;
                var newWhere = new WhereExpression(
                    source,
                    node.ItemName,
                    Expression.And(
                        sourceWhere.Predicate,
                        node.Predicate));

                return base.Visit(newWhere);
            }

            return node;
        }
    }
}
