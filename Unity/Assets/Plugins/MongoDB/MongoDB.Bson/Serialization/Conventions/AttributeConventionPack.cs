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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// Convention pack for applying attributes.
    /// </summary>
    public class AttributeConventionPack : IConventionPack
    {
        // private static fields
        private static readonly AttributeConventionPack __attributeConventionPack = new AttributeConventionPack();

        // private fields
        private readonly AttributeConvention _attributeConvention;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributeConventionPack" /> class.
        /// </summary>
        private AttributeConventionPack()
        {
            _attributeConvention = new AttributeConvention();
        }

        // public static properties
        /// <summary>
        /// Gets the instance.
        /// </summary>
        public static IConventionPack Instance
        {
            get { return __attributeConventionPack; }
        }

        // public properties
        /// <summary>
        /// Gets the conventions.
        /// </summary>
        public IEnumerable<IConvention> Conventions
        {
            get { yield return _attributeConvention; }
        }

        // nested classes
        private class AttributeConvention : ConventionBase, IClassMapConvention, ICreatorMapConvention, IMemberMapConvention, IPostProcessingConvention
        {
            // public methods
            public void Apply(BsonClassMap classMap)
            {
                foreach (var attribute in classMap.ClassType.GetTypeInfo().GetCustomAttributes(inherit: false).OfType<IBsonClassMapAttribute>())
                {
                    attribute.Apply(classMap);
                }

                OptInMembersWithBsonMemberMapModifierAttribute(classMap);
                OptInMembersWithBsonCreatorMapModifierAttribute(classMap);
                IgnoreMembersWithBsonIgnoreAttribute(classMap);
                ThrowForDuplicateMemberMapAttributes(classMap);
            }

            public void Apply(BsonCreatorMap creatorMap)
            {
                if (creatorMap.MemberInfo != null)
                {
                    foreach (var attribute in creatorMap.MemberInfo.GetCustomAttributes(inherit: false).OfType<IBsonCreatorMapAttribute>())
                    {
                        attribute.Apply(creatorMap);
                    }
                }
            }

            public void Apply(BsonMemberMap memberMap)
            {
                var attributes = memberMap.MemberInfo.GetCustomAttributes(inherit: false).OfType<IBsonMemberMapAttribute>();
                var groupings = attributes.GroupBy(a => (a is BsonSerializerAttribute) ? 1 : 2);
                foreach (var grouping in groupings.OrderBy(g => g.Key))
                {
                    foreach (var attribute in grouping)
                    {
                        attribute.Apply(memberMap);
                    }
                }
            }

            public void PostProcess(BsonClassMap classMap)
            {
                foreach (var attribute in classMap.ClassType.GetTypeInfo().GetCustomAttributes(inherit: false).OfType<IBsonPostProcessingAttribute>())
                {
                    attribute.PostProcess(classMap);
                }
            }

            // private methods
            private bool AllowsDuplicate(Type type)
            {
                var usageAttribute = type.GetTypeInfo().GetCustomAttributes(inherit: true)
                    .OfType<BsonMemberMapAttributeUsageAttribute>()
                    .SingleOrDefault();

                return usageAttribute == null || usageAttribute.AllowMultipleMembers;
            }

            private void OptInMembersWithBsonCreatorMapModifierAttribute(BsonClassMap classMap)
            {
                // let other constructors opt-in if they have any IBsonCreatorMapAttribute attributes
                foreach (var constructorInfo in classMap.ClassType.GetTypeInfo().GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    var hasAttribute = constructorInfo.GetCustomAttributes(inherit: false).OfType<IBsonCreatorMapAttribute>().Any();
                    if (hasAttribute)
                    {
                        classMap.MapConstructor(constructorInfo);
                    }
                }

                // let other static factory methods opt-in if they have any IBsonCreatorMapAttribute attributes
                foreach (var methodInfo in classMap.ClassType.GetTypeInfo().GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    var hasAttribute = methodInfo.GetCustomAttributes(inherit: false).OfType<IBsonCreatorMapAttribute>().Any();
                    if (hasAttribute)
                    {
                        classMap.MapFactoryMethod(methodInfo);
                    }
                }
            }

            private void OptInMembersWithBsonMemberMapModifierAttribute(BsonClassMap classMap)
            {
                // let other fields opt-in if they have any IBsonMemberMapAttribute attributes
                foreach (var fieldInfo in classMap.ClassType.GetTypeInfo().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    var hasAttribute = fieldInfo.GetCustomAttributes(inherit: false).OfType<IBsonMemberMapAttribute>().Any();
                    if (hasAttribute)
                    {
                        classMap.MapMember(fieldInfo);
                    }
                }

                // let other properties opt-in if they have any IBsonMemberMapAttribute attributes
                foreach (var propertyInfo in classMap.ClassType.GetTypeInfo().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    var hasAttribute = propertyInfo.GetCustomAttributes(inherit: false).OfType<IBsonMemberMapAttribute>().Any();
                    if (hasAttribute)
                    {
                        classMap.MapMember(propertyInfo);
                    }
                }
            }

            private void IgnoreMembersWithBsonIgnoreAttribute(BsonClassMap classMap)
            {
                foreach (var memberMap in classMap.DeclaredMemberMaps.ToList())
                {
                    var ignoreAttribute = (BsonIgnoreAttribute)memberMap.MemberInfo.GetCustomAttributes(inherit: false).OfType<BsonIgnoreAttribute>().FirstOrDefault();
                    if (ignoreAttribute != null)
                    {
                        classMap.UnmapMember(memberMap.MemberInfo);
                    }
                }
            }

            private void ThrowForDuplicateMemberMapAttributes(BsonClassMap classMap)
            {
                var nonDuplicatesAlreadySeen = new List<Type>();
                foreach (var memberMap in classMap.DeclaredMemberMaps)
                {
                    var attributes = memberMap.MemberInfo.GetCustomAttributes(inherit: false).OfType<IBsonMemberMapAttribute>();
                    // combine them only if the modifier isn't already in the attributes list...
                    var attributeTypes = attributes.Select(x => x.GetType());
                    foreach (var attributeType in attributeTypes)
                    {
                        if (nonDuplicatesAlreadySeen.Contains(attributeType))
                        {
                            var message = string.Format("Attributes of type {0} can only be applied to a single member.", attributeType);
                            throw new DuplicateBsonMemberMapAttributeException(message);
                        }

                        if (!AllowsDuplicate(attributeType))
                        {
                            nonDuplicatesAlreadySeen.Add(attributeType);
                        }
                    }
                }
            }
        }
    }
}