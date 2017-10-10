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
using System.Linq.Expressions;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Linq.Expressions
{
    internal sealed class CollectionExpression : SerializationExpression
    {
        private readonly CollectionNamespace _collectionNamespace;
        private readonly IBsonSerializer _serializer;

        public CollectionExpression(CollectionNamespace collectionNamespace, IBsonSerializer itemSerializer)
        {
            _collectionNamespace = Ensure.IsNotNull(collectionNamespace, nameof(collectionNamespace));
            _serializer = SerializerHelper.CreateEnumerableSerializer(Ensure.IsNotNull(itemSerializer, nameof(itemSerializer)));
        }

        public override ExtensionExpressionType ExtensionType
        {
            get { return ExtensionExpressionType.Collection; }
        }

        public CollectionNamespace CollectionNamespace
        {
            get { return _collectionNamespace; }
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
            return $"[{_collectionNamespace}]";
        }

        protected internal override Expression Accept(ExtensionExpressionVisitor visitor)
        {
            return visitor.VisitCollection(this);
        }
    }
}