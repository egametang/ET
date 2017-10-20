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
    internal sealed class TakeExpression : ExtensionExpression, ISourcedExpression
    {
        private readonly Expression _count;
        private readonly Expression _source;

        public TakeExpression(Expression source, Expression count)
        {
            _source = Ensure.IsNotNull(source, nameof(source));
            _count = Ensure.IsNotNull(count, nameof(count));
        }

        public override ExtensionExpressionType ExtensionType
        {
            get { return ExtensionExpressionType.Take; }
        }

        public Expression Count
        {
            get { return _count; }
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
            return string.Format("{0}.Take({1})", _source.ToString(), _count.ToString());
        }

        public TakeExpression Update(Expression source, Expression count)
        {
            if (source != _source ||
                count != _count)
            {
                return new TakeExpression(source, count);
            }

            return this;
        }

        protected internal override Expression Accept(ExtensionExpressionVisitor visitor)
        {
            return visitor.VisitTake(this);
        }
    }
}
