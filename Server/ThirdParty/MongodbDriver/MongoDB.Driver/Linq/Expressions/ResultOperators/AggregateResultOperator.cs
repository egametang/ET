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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq.Expressions.ResultOperators
{
    internal sealed class AggregateResultOperator : ResultOperator
    {
        private readonly Expression _finalizer;
        private readonly string _itemName;
        private readonly Expression _reducer;
        private readonly Expression _seed;
        private readonly IBsonSerializer _serializer;

        public AggregateResultOperator(Expression seed, Expression reducer, Expression finalizer, string itemName, IBsonSerializer serializer)
        {
            _seed = Ensure.IsNotNull(seed, nameof(seed));
            _reducer = Ensure.IsNotNull(reducer, nameof(reducer));
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));

            _finalizer = finalizer;
            _itemName = itemName;
        }

        public Expression Finalizer
        {
            get { return _finalizer; }
        }

        public string ItemName
        {
            get { return _itemName; }
        }

        public override string Name
        {
            get { return "Aggregate"; }
        }

        public Expression Reducer
        {
            get { return _reducer; }
        }

        public Expression Seed
        {
            get { return _seed; }
        }

        public override IBsonSerializer Serializer
        {
            get { return _serializer; }
        }

        public override Type Type
        {
            get { return _serializer.ValueType; }
        }

        protected internal override ResultOperator Update(ExtensionExpressionVisitor visitor)
        {
            var seed = visitor.Visit(_seed);
            var reducer = visitor.Visit(_reducer);
            Expression finalizer = null;
            if (_finalizer != null)
            {
                finalizer = visitor.Visit(_finalizer);
            }
            if (seed != _seed || reducer != _reducer || finalizer != _finalizer)
            {
                return new AggregateResultOperator(seed, reducer, finalizer, _itemName, _serializer);
            }

            return this;
        }
    }
}
