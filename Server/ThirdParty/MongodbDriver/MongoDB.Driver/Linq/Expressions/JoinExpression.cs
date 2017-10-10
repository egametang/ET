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
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq.Expressions
{
    internal sealed class JoinExpression : ExtensionExpression, ISourcedExpression
    {
        private readonly Type _type;
        private readonly Expression _source;
        private readonly Expression _joined;
        private readonly Expression _sourceKeySelector;
        private readonly Expression _joinedKeySelector;
        private readonly string _joinedItemName;

        public JoinExpression(Type type, Expression source, Expression joined, Expression sourceKeySelector, Expression joinedKeySelector, string joinedItemName)
        {
            _type = Ensure.IsNotNull(type, nameof(type));
            _source = Ensure.IsNotNull(source, nameof(source));
            _joined = Ensure.IsNotNull(joined, nameof(joined));
            _sourceKeySelector = Ensure.IsNotNull(sourceKeySelector, nameof(sourceKeySelector));
            _joinedKeySelector = Ensure.IsNotNull(joinedKeySelector, nameof(joinedKeySelector));
            _joinedItemName = Ensure.IsNotNull(joinedItemName, nameof(joinedItemName));
        }

        public override ExtensionExpressionType ExtensionType => ExtensionExpressionType.Join;

        public Expression Joined => _joined;

        public Expression JoinedKeySelector => _joinedKeySelector;

        public string JoinedItemName => _joinedItemName;

        public Expression Source => _source;

        public Expression SourceKeySelector => _sourceKeySelector;

        public override Type Type => _type;

        protected internal override Expression Accept(ExtensionExpressionVisitor visitor)
        {
            return visitor.VisitJoin(this);
        }

        public Expression Update(Expression source, Expression joined, Expression sourceKeySelector, Expression joinedKeySelector)
        {
            if (source != _source ||
                joined != _joined ||
                sourceKeySelector != _sourceKeySelector ||
                joinedKeySelector != _joinedKeySelector)
            {
                return new JoinExpression(_type, source, joined, sourceKeySelector, joinedKeySelector, _joinedItemName);
            }

            return this;
        }
    }
}
