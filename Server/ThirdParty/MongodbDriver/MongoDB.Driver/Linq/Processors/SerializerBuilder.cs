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
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Expressions;

namespace MongoDB.Driver.Linq.Processors
{
    internal sealed class SerializerBuilder
    {
        public static IBsonSerializer Build(Expression node, IBsonSerializerRegistry serializerRegistry)
        {
            var builder = new SerializerBuilder(serializerRegistry);
            return builder.Build(node);
        }

        private IBsonSerializerRegistry _serializerRegistry;

        private SerializerBuilder(IBsonSerializerRegistry serializerRegistry)
        {
            _serializerRegistry = serializerRegistry;
        }

        public IBsonSerializer Build(Expression node)
        {
            if (node is ISerializationExpression)
            {
                return ((ISerializationExpression)node).Serializer;
            }

            IBsonSerializer serializer = null;
            switch (node.NodeType)
            {
                case ExpressionType.MemberInit:
                    serializer = BuildMemberInit((MemberInitExpression)node);
                    break;
                case ExpressionType.New:
                    if (!typeof(IEnumerable).GetTypeInfo().IsAssignableFrom(node.Type))
                    {
                        serializer = BuildNew((NewExpression)node);
                    }
                    break;
            }

            if (serializer == null && !PreviouslyUsedSerializerFinder.TryFindSerializer(node, node.Type, out serializer))
            {
                serializer = _serializerRegistry.GetSerializer(node.Type);
                var childConfigurable = serializer as IChildSerializerConfigurable;
                if (childConfigurable != null)
                {
                    var arraySerializer = serializer as IBsonArraySerializer;
                    BsonSerializationInfo itemSerializationInfo;
                    if (arraySerializer != null && arraySerializer.TryGetItemSerializationInfo(out itemSerializationInfo))
                    {
                        IBsonSerializer itemSerializer;
                        if (PreviouslyUsedSerializerFinder.TryFindSerializer(node, itemSerializationInfo.Serializer.ValueType, out itemSerializer))
                        {
                            serializer = SerializerHelper.RecursiveConfigureChildSerializer(childConfigurable, itemSerializer);
                        }
                    }
                    else
                    {
                        IBsonSerializer childSerializer;
                        if (PreviouslyUsedSerializerFinder.TryFindSerializer(node, childConfigurable.ChildSerializer.ValueType, out childSerializer))
                        {
                            serializer = childConfigurable.WithChildSerializer(childSerializer);
                        }
                    }
                }
            }

            return serializer;
        }



        private IBsonSerializer BuildMemberInit(MemberInitExpression node)
        {
            var mapping = ProjectionMapper.Map(node);
            return BuildProjectedSerializer(mapping);
        }

        private IBsonSerializer BuildNew(NewExpression node)
        {
            var mapping = ProjectionMapper.Map(node);
            return BuildProjectedSerializer(mapping);
        }

        private IBsonSerializer BuildProjectedSerializer(ProjectionMapping mapping)
        {
            // We are building a serializer specifically for a projected type based
            // on serialization information collected from other serializers.
            // We cannot cache this in the serializer registry because the compiler reuses 
            // the same anonymous type definition in different contexts as long as they 
            // are structurally equatable. As such, it might be that two different queries 
            // projecting the same shape might need to be deserialized differently.
            var classMap = BuildClassMap(mapping.Expression.Type, mapping);

            var mappedParameters = mapping.Members
                .Where(x => x.Parameter != null)
                .OrderBy(x => x.Parameter.Position)
                .Select(x => x.Member)
                .ToList();

            if (mappedParameters.Count > 0)
            {
                classMap.MapConstructor(mapping.Constructor)
                    .SetArguments(mappedParameters);
            }

            var serializerType = typeof(BsonClassMapSerializer<>).MakeGenericType(mapping.Expression.Type);
            return (IBsonSerializer)Activator.CreateInstance(serializerType, classMap.Freeze());
        }

        private BsonClassMap BuildClassMap(Type type, ProjectionMapping mapping)
        {
            if (type == null || type == typeof(object))
            {
                return null;
            }

            var baseClassMap = BuildClassMap(type.GetTypeInfo().BaseType, mapping);
            if (baseClassMap != null)
            {
                baseClassMap.Freeze();
            }
            var classMap = new BsonClassMap(type, baseClassMap);

            foreach (var memberMapping in mapping.Members.Where(x => x.Member.DeclaringType == type))
            {
                var serializationExpression = memberMapping.Expression as SerializationExpression;
                if (serializationExpression == null)
                {
                    var serializer = Build(memberMapping.Expression);
                    serializationExpression = new FieldExpression(
                        memberMapping.Member.Name,
                        serializer,
                        memberMapping.Expression);
                }

                var memberMap = classMap.MapMember(memberMapping.Member)
                    .SetSerializer(serializationExpression.Serializer)
                    .SetElementName(memberMapping.Member.Name);

                if (classMap.IdMemberMap == null && serializationExpression is GroupingKeyExpression)
                {
                    classMap.SetIdMember(memberMap);
                }
            }

            return classMap;
        }

        private static Type GetMemberType(MemberInfo memberInfo)
        {
            FieldInfo fieldInfo;
            if ((fieldInfo = memberInfo as FieldInfo) != null)
            {
                return fieldInfo.FieldType;
            }

            PropertyInfo propertyInfo;
            if ((propertyInfo = memberInfo as PropertyInfo) != null)
            {
                return propertyInfo.PropertyType;
            }

            throw new MongoInternalException("Can't get member type.");
        }
    }
}