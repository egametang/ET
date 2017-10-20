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
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors
{
    internal sealed class PreviouslyUsedSerializerFinder : ExtensionExpressionVisitor
    {
        public static bool TryFindSerializer(Expression node, Type type, out IBsonSerializer serializer)
        {
            var finder = new PreviouslyUsedSerializerFinder(type);
            finder.Visit(node);

            serializer = finder._serializer;
            return serializer != null;
        }

        private readonly Type _valueType;
        private IBsonSerializer _serializer;

        private PreviouslyUsedSerializerFinder(Type valueType)
        {
            _valueType = valueType;
        }

        public override Expression Visit(Expression expression)
        {
            if (_serializer != null)
            {
                return expression;
            }

            var serializationExpression = expression as ISerializationExpression;
            if (serializationExpression != null && serializationExpression.Serializer.ValueType == _valueType)
            {
                _serializer = serializationExpression.Serializer;
                return expression;
            }

            return base.Visit(expression);
        }
    }
}
