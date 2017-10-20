/* Copyright 2010-2016 MongoDB Inc.
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
using System.Reflection;

namespace MongoDB.Bson.Serialization.Attributes
{
    /// <summary>
    /// Specifies the type of the serializer to use for a class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Property | AttributeTargets.Field)]
    public class BsonSerializerAttribute : Attribute, IBsonMemberMapAttribute
    {
        // private fields
        private Type _serializerType;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonSerializerAttribute class.
        /// </summary>
        public BsonSerializerAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonSerializerAttribute class.
        /// </summary>
        /// <param name="serializerType">The type of the serializer to use for a class.</param>
        public BsonSerializerAttribute(Type serializerType)
        {
            _serializerType = serializerType;
        }

        // public properties
        /// <summary>
        /// Gets or sets the type of the serializer to use for a class.
        /// </summary>
        public Type SerializerType
        {
            get { return _serializerType; }
            set { _serializerType = value; }
        }

        // public methods
        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// <param name="memberMap">The member map.</param>
        public void Apply(BsonMemberMap memberMap)
        {
            var serializer = CreateSerializer(memberMap.MemberType);
            memberMap.SetSerializer(serializer);
        }

        /// <summary>
        /// Creates a serializer for a type based on the serializer type specified by the attribute.
        /// </summary>
        /// <param name="type">The type that a serializer should be created for.</param>
        /// <returns>A serializer for the type.</returns>
        internal IBsonSerializer CreateSerializer(Type type)
        {
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.ContainsGenericParameters)
            {
                var message = "Cannot create a serializer because the type to serialize is an open generic type.";
                throw new InvalidOperationException(message);
            }

            var serializerTypeInfo = _serializerType.GetTypeInfo();
            if (serializerTypeInfo.ContainsGenericParameters && !typeInfo.IsGenericType)
            {
                var message = "Cannot create a serializer because the serializer type is an open generic type and the type to serialize is not generic.";
                throw new InvalidOperationException(message);
            }

            if (serializerTypeInfo.ContainsGenericParameters)
            {
                var genericArguments = typeInfo.GetGenericArguments();
                var closedSerializerType = _serializerType.MakeGenericType(genericArguments);
                return (IBsonSerializer)Activator.CreateInstance(closedSerializerType);
            }
            else
            {
                return (IBsonSerializer)Activator.CreateInstance(_serializerType);
            }
        }
    }
}
