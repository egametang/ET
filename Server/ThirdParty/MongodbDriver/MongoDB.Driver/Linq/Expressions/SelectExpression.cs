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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq.Expressions
{
    internal sealed class SelectExpression : ExtensionExpression, ISourcedExpression
    {
        private readonly string _itemName;
        private readonly Expression _selector;
        private readonly Expression _source;

        public SelectExpression(Expression source, string itemName, Expression selector)
        {
            _source = Ensure.IsNotNull(source, nameof(source));
            _itemName = Ensure.IsNotNull(itemName, nameof(itemName));
            _selector = Ensure.IsNotNull(selector, nameof(selector));
        }

        public override ExtensionExpressionType ExtensionType
        {
            get { return ExtensionExpressionType.Select; }
        }

        public Expression Selector
        {
            get { return _selector; }
        }

        public Expression Source
        {
            get { return _source; }
        }

        public string ItemName
        {
            get { return _itemName; }
        }

        public override Type Type
        {
            get { return _source.Type; }
        }

        public override string ToString()
        {
            return string.Format("{0}.Select({1})", _source.ToString(), _selector.ToString());
        }

        public SelectExpression Update(Expression source, Expression selector)
        {
            if (source != _source ||
                selector != _selector)
            {
                return new SelectExpression(source, _itemName, selector);
            }

            return this;
        }

        protected internal override Expression Accept(ExtensionExpressionVisitor visitor)
        {
            return visitor.VisitSelect(this);
        }
    }
}
