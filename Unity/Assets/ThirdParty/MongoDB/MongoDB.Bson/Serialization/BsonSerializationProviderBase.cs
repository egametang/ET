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

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Base class for serialization providers.
    /// </summary>
    public abstract class BsonSerializationProviderBase : IRegistryAwareBsonSerializationProvider
    {
        /// <inheritdoc/>
        public virtual IBsonSerializer GetSerializer(Type type)
        {
            return GetSerializer(type, BsonSerializer.SerializerRegistry);
        }

        /// <inheritdoc/>
        public abstract IBsonSerializer GetSerializer(Type type, IBsonSerializerRegistry serializerRegistry);

        /// <summary>
        /// Creates the serializer from a serializer type definition and type arguments.
        /// </summary>
        /// <param name="serializerTypeDefinition">The serializer type definition.</param>
        /// <param name="typeArguments">The type arguments.</param>
        /// <returns>A serializer.</returns>
        protected virtual IBsonSerializer CreateGenericSerializer(Type serializerTypeDefinition, params Type[] typeArguments)
        {
            return CreateGenericSerializer(serializerTypeDefinition, typeArguments, BsonSerializer.SerializerRegistry);
        }

        /// <summary>
        /// Creates the serializer from a serializer type definition and type arguments.
        /// </summary>
        /// <param name="serializerTypeDefinition">The serializer type definition.</param>
        /// <param name="typeArguments">The type arguments.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>
        /// A serializer.
        /// </returns>
        protected virtual IBsonSerializer CreateGenericSerializer(Type serializerTypeDefinition, Type[] typeArguments, IBsonSerializerRegistry serializerRegistry)
        {
            var serializerType = serializerTypeDefinition.MakeGenericType(typeArguments);
            return CreateSerializer(serializerType, serializerRegistry);
        }

        /// <summary>
        /// Creates the serializer.
        /// </summary>
        /// <param name="serializerType">The serializer type.</param>
        /// <returns>A serializer.</returns>
        protected virtual IBsonSerializer CreateSerializer(Type serializerType)
        {
            return CreateSerializer(serializerType, BsonSerializer.SerializerRegistry);
        }

        /// <summary>
        /// Creates the serializer.
        /// </summary>
        /// <param name="serializerType">The serializer type.</param>
        /// <param name="serializerRegistry">The serializer registry.</param>
        /// <returns>
        /// A serializer.
        /// </returns>
        protected virtual IBsonSerializer CreateSerializer(Type serializerType, IBsonSerializerRegistry serializerRegistry)
        {
            var serializerTypeInfo = serializerType.GetTypeInfo();
            var constructorInfo = serializerTypeInfo.GetConstructor(new[] { typeof(IBsonSerializerRegistry) });
            if (constructorInfo != null)
            {
                return (IBsonSerializer)constructorInfo.Invoke(new object[] { serializerRegistry });
            }

            constructorInfo = serializerTypeInfo.GetConstructor(new Type[0]);
            if (constructorInfo != null)
            {
                return (IBsonSerializer)constructorInfo.Invoke(new object[0]);
            }

            throw new MissingMethodException(string.Format(
                "No suitable constructor found for serializer type: '{0}'.",
                serializerType.FullName));
        }
    }
}
