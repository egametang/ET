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
using System.Collections.Generic;
using System.Linq.Expressions;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq.Expressions
{
    internal sealed class GroupByWithResultSelectorExpression : ExtensionExpression, ISourcedExpression
    {
        private readonly Expression _selector;
        private readonly Expression _source;
        private readonly Type _type;

        public GroupByWithResultSelectorExpression(Expression source, Expression selector)
        {
            _source = Ensure.IsNotNull(source, nameof(source));
            _selector = Ensure.IsNotNull(selector, nameof(selector));
            _type = typeof(IEnumerable<>).MakeGenericType(selector.Type);
        }

        public override ExtensionExpressionType ExtensionType
        {
            get { return ExtensionExpressionType.GroupByWithResultSelector; }
        }

        public Expression Selector
        {
            get { return _selector; }
        }

        public Expression Source
        {
            get { return _source; }
        }

        public override Type Type
        {
            get { return _type; }
        }

        public override string ToString()
        {
            return string.Format("{0}.GroupBy({1})", _source.ToString(), _selector.ToString());
        }

        public GroupByWithResultSelectorExpression Update(Expression source, Expression selector)
        {
            if (source != _source ||
                selector != _selector)
            {
                return new GroupByWithResultSelectorExpression(source, selector);
            }

            return this;
        }

        protected internal override Expression Accept(ExtensionExpressionVisitor visitor)
        {
            return visitor.VisitGroupByWithResultSelector(this);
        }
    }
}
