/* Copyright 2016 MongoDB Inc.
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
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq.Expressions
{
    internal sealed class InjectedFilterExpression : ExtensionExpression
    {
        private readonly BsonDocument _filter;

        public InjectedFilterExpression(BsonDocument filter)
        {
            _filter = Ensure.IsNotNull(filter, nameof(filter));
        }

        public override ExtensionExpressionType ExtensionType => ExtensionExpressionType.InjectedFilter;

        public BsonDocument Filter => _filter;

        public override Type Type => typeof(bool);

        public override string ToString() => _filter.ToString();

        protected internal override Expression Accept(ExtensionExpressionVisitor visitor)
        {
            return visitor.VisitInjectedFilter(this);
        }
    }
}