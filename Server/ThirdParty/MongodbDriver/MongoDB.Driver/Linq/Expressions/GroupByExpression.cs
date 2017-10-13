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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq.Expressions
{
    internal sealed class GroupByExpression : ExtensionExpression, ISourcedExpression
    {
        private readonly Expression _keySelector;
        private readonly Expression _source;
        private readonly ReadOnlyCollection<AccumulatorExpression> _accumulators;
        private readonly Type _type;

        public GroupByExpression(Expression source, Expression keySelector)
            : this(source.Type, source, keySelector, Enumerable.Empty<AccumulatorExpression>())
        {
        }

        public GroupByExpression(Type type, Expression source, Expression keySelector, IEnumerable<AccumulatorExpression> accumulators)
        {
            _type = Ensure.IsNotNull(type, nameof(type));
            _source = Ensure.IsNotNull(source, nameof(source));
            _keySelector = Ensure.IsNotNull(keySelector, nameof(keySelector));
            _accumulators = Ensure.IsNotNull(accumulators, "accumulators") as ReadOnlyCollection<AccumulatorExpression>;
            if (_accumulators == null)
            {
                _accumulators = accumulators.ToList().AsReadOnly();
            }
        }

        public ReadOnlyCollection<AccumulatorExpression> Accumulators
        {
            get { return _accumulators; }
        }

        public override ExtensionExpressionType ExtensionType
        {
            get { return ExtensionExpressionType.GroupBy; }
        }

        public Expression KeySelector
        {
            get { return _keySelector; }
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
            return string.Format("{0}.GroupBy({1})", _source.ToString(), _keySelector.ToString());
        }

        public GroupByExpression Update(Expression source, Expression keySelector, IEnumerable<AccumulatorExpression> accumulators)
        {
            if (source != _source ||
                keySelector != _keySelector ||
                accumulators != _accumulators)
            {
                return new GroupByExpression(_type, source, keySelector, accumulators);
            }

            return this;
        }

        protected internal override Expression Accept(ExtensionExpressionVisitor visitor)
        {
            return visitor.VisitGroupBy(this);
        }
    }
}
