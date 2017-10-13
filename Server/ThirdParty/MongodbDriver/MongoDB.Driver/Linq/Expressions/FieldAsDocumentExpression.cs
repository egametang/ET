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

using System;
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq.Expressions
{
    internal sealed class FieldAsDocumentExpression : SerializationExpression, IFieldExpression
    {
        private readonly Expression _expression;
        private readonly string _fieldName;
        private readonly IBsonSerializer _serializer;

        public FieldAsDocumentExpression(Expression expression, string fieldName, IBsonSerializer serializer)
        {
            _expression = Ensure.IsNotNull(expression, nameof(expression));
            _fieldName = Ensure.IsNotNull(fieldName, nameof(fieldName));
            _serializer = Ensure.IsNotNull(serializer, nameof(serializer));
        }

        public Expression Document
        {
            get { return null; }
        }

        public Expression Expression
        {
            get { return _expression; }
        }

        public string FieldName
        {
            get { return _fieldName; }
        }

        public override ExtensionExpressionType ExtensionType
        {
            get { return ExtensionExpressionType.FieldAsDocument; }
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
            return "{" + _fieldName + "}";
        }

        public FieldAsDocumentExpression Update(Expression expression)
        {
            if (expression != _expression)
            {
                return new FieldAsDocumentExpression(expression, _fieldName, _serializer);
            }

            return this;
        }

        protected internal override Expression Accept(ExtensionExpressionVisitor visitor)
        {
            return visitor.VisitDocumentWrappedField(this);
        }
    }
}