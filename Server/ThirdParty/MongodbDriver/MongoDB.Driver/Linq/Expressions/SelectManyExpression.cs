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
    internal sealed class SelectManyExpression : ExtensionExpression, ISourcedExpression
    {
        private readonly string _collectionItemName;
        private readonly Expression _collectionSelector;
        private readonly string _resultItemName;
        private readonly Expression _resultSelector;
        private readonly Expression _source;

        public SelectManyExpression(Expression source, string collectionItemName, Expression collectionSelector, string resultItemName, Expression resultSelector)
        {
            _source = Ensure.IsNotNull(source, nameof(source));
            _collectionItemName = Ensure.IsNotNull(collectionItemName, nameof(collectionItemName));
            _collectionSelector = Ensure.IsNotNull(collectionSelector, nameof(collectionSelector));
            _resultItemName = Ensure.IsNotNull(resultItemName, nameof(resultItemName));
            _resultSelector = Ensure.IsNotNull(resultSelector, nameof(resultSelector));
        }

        public override ExtensionExpressionType ExtensionType
        {
            get { return ExtensionExpressionType.SelectMany; }
        }

        public string CollectionItemName
        {
            get { return _collectionItemName; }
        }

        public Expression CollectionSelector
        {
            get { return _collectionSelector; }
        }

        public string ResultItemName
        {
            get { return _resultItemName; }
        }

        public Expression ResultSelector
        {
            get { return _resultSelector; }
        }

        public Expression Source
        {
            get { return _source; }
        }

        public override Type Type
        {
            get { return _source.Type; }
        }

        public override string ToString()
        {
            return string.Format("{0}.SelectMany({1})", _source.ToString(), _resultSelector.ToString());
        }

        public SelectManyExpression Update(Expression source, Expression collectionSelector, Expression resultSelector)
        {
            if (source != _source ||
                collectionSelector != _collectionSelector ||
                resultSelector != _resultSelector)
            {
                return new SelectManyExpression(
                    source,
                    _collectionItemName,
                    collectionSelector,
                    _resultItemName,
                    resultSelector);
            }

            return this;
        }

        protected internal override Expression Accept(ExtensionExpressionVisitor visitor)
        {
            return visitor.VisitSelectMany(this);
        }
    }
}
