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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Reflection;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Provides serializers for collections.
    /// </summary>
    public class CollectionsSerializationProvider : BsonSerializationProviderBase
    {
        private static readonly Dictionary<Type, Type> __serializerTypes;

        static CollectionsSerializationProvider()
        {
            __serializerTypes = new Dictionary<Type, Type>
            {
                { typeof(BitArray), typeof(BitArraySerializer) },
                { typeof(ExpandoObject), typeof(ExpandoObjectSerializer) },
                { typeof(Queue), typeof(QueueSerializer) },
                { typeof(Stack), typeof(StackSerializer) },
                { typeof(Queue<>), typeof(QueueSerializer<>) },
                { typeof(ReadOnlyCollection<>), typeof(ReadOnlyCollectionSerializer<>) },
                { typeof(Stack<>), typeof(StackSerializer<>) },
            };
        }

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

            Type serializerType;
            if (__serializerTypes.TryGetValue(type, out serializerType))
            {
                return CreateSerializer(serializerType, serializerRegistry);
            }

            if (typeInfo.IsGenericType && !typeInfo.ContainsGenericParameters)
            {
                Type serializerTypeDefinition;
                if (__serializerTypes.TryGetValue(type.GetGenericTypeDefinition(), out serializerTypeDefinition))
                {
                    return CreateGenericSerializer(serializerTypeDefinition, type.GetTypeInfo().GetGenericArguments(), serializerRegistry);
                }
            }

            if (type.IsArray)
            {
                var elementType = type.GetElementType();
                switch (type.GetArrayRank())
                {
                    case 1:
                        var arraySerializerDefinition = typeof(ArraySerializer<>);
                        return CreateGenericSerializer(arraySerializerDefinition, new[] { elementType }, serializerRegistry);
                    case 2:
                        var twoDimensionalArraySerializerDefinition = typeof(TwoDimensionalArraySerializer<>);
                        return CreateGenericSerializer(twoDimensionalArraySerializerDefinition, new[] { elementType }, serializerRegistry);
                    case 3:
                        var threeDimensionalArraySerializerDefinition = typeof(ThreeDimensionalArraySerializer<>);
                        return CreateGenericSerializer(threeDimensionalArraySerializerDefinition, new[] { elementType }, serializerRegistry);
                    default:
                        var message = string.Format("No serializer found for array for rank {0}.", type.GetArrayRank());
                        throw new BsonSerializationException(message);
                }
            }

            return GetCollectionSerializer(type, serializerRegistry);
        }

        private IBsonSerializer GetCollectionSerializer(Type type, IBsonSerializerRegistry serializerRegistry)
        {
            Type implementedGenericDictionaryInterface = null;
            Type implementedGenericEnumerableInterface = null;
            Type implementedGenericSetInterface = null;
            Type implementedDictionaryInterface = null;
            Type implementedEnumerableInterface = null;

            var implementedInterfaces = new List<Type>(type.GetTypeInfo().GetInterfaces());
            var typeInfo = type.GetTypeInfo();
            if (typeInfo.IsInterface)
            {
                implementedInterfaces.Add(type);
            }

            foreach (var implementedInterface in implementedInterfaces)
            {
                var implementedInterfaceTypeInfo = implementedInterface.GetTypeInfo();
                if (implementedInterfaceTypeInfo.IsGenericType)
                {
                    var genericInterfaceDefinition = implementedInterface.GetGenericTypeDefinition();
                    if (genericInterfaceDefinition == typeof(IDictionary<,>))
                    {
                        implementedGenericDictionaryInterface = implementedInterface;
                    }
                    if (genericInterfaceDefinition == typeof(IEnumerable<>))
                    {
                        implementedGenericEnumerableInterface = implementedInterface;
                    }
                    if (genericInterfaceDefinition == typeof(ISet<>))
                    {
                        implementedGenericSetInterface = implementedInterface;
                    }
                }
                else
                {
                    if (implementedInterface == typeof(IDictionary))
                    {
                        implementedDictionaryInterface = implementedInterface;
                    }
                    if (implementedInterface == typeof(IEnumerable))
                    {
                        implementedEnumerableInterface = implementedInterface;
                    }
                }
            }

            // the order of the tests is important
            if (implementedGenericDictionaryInterface != null)
            {
                var keyType = implementedGenericDictionaryInterface.GetTypeInfo().GetGenericArguments()[0];
                var valueType = implementedGenericDictionaryInterface.GetTypeInfo().GetGenericArguments()[1];
                if (typeInfo.IsInterface)
                {
                    var dictionaryDefinition = typeof(Dictionary<,>);
                    var dictionaryType = dictionaryDefinition.MakeGenericType(keyType, valueType);
                    var serializerDefinition = typeof(ImpliedImplementationInterfaceSerializer<,>);
                    return CreateGenericSerializer(serializerDefinition, new[] { type, dictionaryType }, serializerRegistry);
                }
                else
                {
                    var serializerDefinition = typeof(DictionaryInterfaceImplementerSerializer<,,>);
                    return CreateGenericSerializer(serializerDefinition, new[] { type, keyType, valueType }, serializerRegistry);
                }
            }
            else if (implementedDictionaryInterface != null)
            {
                if (typeInfo.IsInterface)
                {
                    var dictionaryType = typeof(Hashtable);
                    var serializerDefinition = typeof(ImpliedImplementationInterfaceSerializer<,>);
                    return CreateGenericSerializer(serializerDefinition, new[] { type, dictionaryType }, serializerRegistry);
                }
                else
                {
                    var serializerDefinition = typeof(DictionaryInterfaceImplementerSerializer<>);
                    return CreateGenericSerializer(serializerDefinition, new[] { type }, serializerRegistry);
                }
            }
            else if (implementedGenericSetInterface != null)
            {
                var itemType = implementedGenericSetInterface.GetTypeInfo().GetGenericArguments()[0];

                if (typeInfo.IsInterface)
                {
                    var hashSetDefinition = typeof(HashSet<>);
                    var hashSetType = hashSetDefinition.MakeGenericType(itemType);
                    var serializerDefinition = typeof(ImpliedImplementationInterfaceSerializer<,>);
                    return CreateGenericSerializer(serializerDefinition, new[] { type, hashSetType }, serializerRegistry);
                }
                else
                {
                    var serializerDefinition = typeof(EnumerableInterfaceImplementerSerializer<,>);
                    return CreateGenericSerializer(serializerDefinition, new[] { type, itemType }, serializerRegistry);
                }
            }
            else if (implementedGenericEnumerableInterface != null)
            {
                var itemType = implementedGenericEnumerableInterface.GetTypeInfo().GetGenericArguments()[0];

                var readOnlyCollectionType = typeof(ReadOnlyCollection<>).MakeGenericType(itemType);
                if (type == readOnlyCollectionType)
                {
                    var serializerDefinition = typeof(ReadOnlyCollectionSerializer<>);
                    return CreateGenericSerializer(serializerDefinition, new[] { itemType }, serializerRegistry);
                }
                else if (readOnlyCollectionType.GetTypeInfo().IsAssignableFrom(type))
                {
                    var serializerDefinition = typeof(ReadOnlyCollectionSubclassSerializer<,>);
                    return CreateGenericSerializer(serializerDefinition, new[] { type, itemType }, serializerRegistry);
                }
                else if (typeInfo.IsInterface)
                {
                    var listDefinition = typeof(List<>);
                    var listType = listDefinition.MakeGenericType(itemType);
                    var serializerDefinition = typeof(ImpliedImplementationInterfaceSerializer<,>);
                    return CreateGenericSerializer(serializerDefinition, new[] { type, listType }, serializerRegistry);
                }
                else
                {
                    var serializerDefinition = typeof(EnumerableInterfaceImplementerSerializer<,>);
                    return CreateGenericSerializer(serializerDefinition, new[] { type, itemType }, serializerRegistry);
                }
            }
            else if (implementedEnumerableInterface != null)
            {
                if (typeInfo.IsInterface)
                {
                    var listType = typeof(ArrayList);
                    var serializerDefinition = typeof(ImpliedImplementationInterfaceSerializer<,>);
                    return CreateGenericSerializer(serializerDefinition, new[] { type, listType }, serializerRegistry);
                }
                else
                {
                    var serializerDefinition = typeof(EnumerableInterfaceImplementerSerializer<>);
                    return CreateGenericSerializer(serializerDefinition, new[] { type }, serializerRegistry);
                }
            }

            return null;
        }
    }
}
