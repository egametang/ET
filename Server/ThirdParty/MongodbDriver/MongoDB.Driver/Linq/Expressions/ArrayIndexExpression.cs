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
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq.Expressions
{
    internal sealed class ArrayIndexExpression : SerializationExpression
    {
        private readonly Expression _array;
        private readonly Expression _index;
        private readonly Expression _original;
        private readonly IBsonSerializer _serializer;

        public ArrayIndexExpression(Expression array, Expression index, IBsonSerializer serializer)
            : this(array, index, serializer, null)
        {
        }

        public ArrayIndexExpression(Expression array, Expression index, IBsonSerializer serializer, Expression original)
        {
            _array = Ensure.IsNotNull(array, nameof(array));
            _index = Ensure.IsNotNull(index, nameof(index));
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
            _original = original;
        }

        public Expression Array
        {
            get { return _array; }
        }

        public Expression Index
        {
            get { return _index; }
        }

        public override ExtensionExpressionType ExtensionType
        {
            get { return ExtensionExpressionType.ArrayIndex; }
        }

        public Expression Original
        {
            get { return _original; }
        }

        public override IBsonSerializer Serializer
        {
            get { return _serializer; }
        }

        public override Type Type
        {
            get { return _serializer.ValueType; }
        }

        public override string ToString()
        {
            return _array.ToString() + "[" + _index.ToString() + "]";
        }

        public ArrayIndexExpression Update(Expression array, Expression index, Expression original)
        {
            if (array != _array || index != _index || original != _original)
            {
                return new ArrayIndexExpression(array, index, _serializer, original);
            }

            return this;
        }

        protected internal override Expression Accept(ExtensionExpressionVisitor visitor)
        {
            return visitor.VisitArrayIndex(this);
        }
    }
}