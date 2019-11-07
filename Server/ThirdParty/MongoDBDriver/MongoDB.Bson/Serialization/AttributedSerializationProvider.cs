﻿/* Copyright 2010-present MongoDB Inc.
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
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Provides serializers based on an attribute.
    /// </summary>
    public class AttributedSerializationProvider : BsonSerializationProviderBase
    {
        /// <inheritdoc/>
        public override IBsonSerializer GetSerializer(Type type, IBsonSerializerRegistry serializerRegistry)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsGenericType && typeInfo.ContainsGenericParameters)
            {
                var message = string.Format("Generic type {0} has unassigned type parameters.", BsonUtils.GetFriendlyTypeName(type));
                throw new ArgumentException(message, "type");
            }

            var serializerAttributes = typeInfo.GetCustomAttributes<BsonSerializerAttribute>(false).ToArray();
            if (serializerAttributes.Length == 1)
            {
                var serializerAttribute = (BsonSerializerAttribute)serializerAttributes[0];
                return serializerAttribute.CreateSerializer(type);
            }

            return null;
        }
    }
}
