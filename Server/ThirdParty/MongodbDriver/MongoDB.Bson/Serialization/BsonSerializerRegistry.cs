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
using System.Collections.Concurrent;
using System.Reflection;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Default, global implementation of an <see cref="IBsonSerializerRegistry"/>.
    /// </summary>
    public sealed class BsonSerializerRegistry : IBsonSerializerRegistry
    {
        // private fields
        private readonly ConcurrentDictionary<Type, IBsonSerializer> _cache;
        private readonly ConcurrentStack<IBsonSerializationProvider> _serializationProviders;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BsonSerializerRegistry"/> class.
        /// </summary>
        public BsonSerializerRegistry()
        {
            _cache = new ConcurrentDictionary<Type,IBsonSerializer>();
            _serializationProviders = new ConcurrentStack<IBsonSerializationProvider>();
        }

        // public methods
        /// <summary>
        /// Gets the serializer for the specified <paramref name="type" />.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        /// The serializer.
        /// </returns>
        public IBsonSerializer GetSerializer(Type type)
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

            return _cache.GetOrAdd(type, CreateSerializer);
        }

        /// <summary>
        /// Gets the serializer for the specified <typeparamref name="T" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>
        /// The serializer.
        /// </returns>
        public IBsonSerializer<T> GetSerializer<T>()
        {
            return (IBsonSerializer<T>)GetSerializer(typeof(T));
        }

        /// <summary>
        /// Registers the serializer.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <param name="serializer">The serializer.</param>
        public void RegisterSerializer(Type type, IBsonSerializer serializer)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            if (serializer == null)
            {
                throw new ArgumentNullException("serializer");
            }
            var typeInfo = type.GetTypeInfo();
            if (typeof(BsonValue).GetTypeInfo().IsAssignableFrom(type))
            {
                var message = string.Format("A serializer cannot be registered for type {0} because it is a subclass of BsonValue.", BsonUtils.GetFriendlyTypeName(type));
                throw new BsonSerializationException(message);
            }
            if (typeInfo.IsGenericType && typeInfo.ContainsGenericParameters)
            {
                var message = string.Format("Generic type {0} has unassigned type parameters.", BsonUtils.GetFriendlyTypeName(type));
                throw new ArgumentException(message, "type");
            }

            if (!_cache.TryAdd(type, serializer))
            {
                var message = string.Format("There is already a serializer registered for type {0}.", BsonUtils.GetFriendlyTypeName(type));
                throw new BsonSerializationException(message);
            }
        }

        /// <summary>
        /// Registers the serialization provider. This behaves like a stack, so the 
        /// last provider registered is the first provider consulted.
        /// </summary>
        /// <param name="serializationProvider">The serialization provider.</param>
        public void RegisterSerializationProvider(IBsonSerializationProvider serializationProvider)
        {
            if (serializationProvider == null)
            {
                throw new ArgumentNullException("serializationProvider");
            }
            
            _serializationProviders.Push(serializationProvider);
        }

        // private methods
        private IBsonSerializer CreateSerializer(Type type)
        {
            foreach (var serializationProvider in _serializationProviders)
            {
                IBsonSerializer serializer;

                var registryAwareSerializationProvider = serializationProvider as IRegistryAwareBsonSerializationProvider;
                if (registryAwareSerializationProvider != null)
                {
                    serializer = registryAwareSerializationProvider.GetSerializer(type, this);
                }
                else
                {
                    serializer = serializationProvider.GetSerializer(type);
                }

                if (serializer != null)
                {
                    return serializer;
                }
            }

            var message = string.Format("No serializer found for type {0}.", type.FullName);
            throw new BsonSerializationException(message);
        }
    }
}
