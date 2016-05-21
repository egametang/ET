/* Copyright 2010-2014 MongoDB Inc.
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
                foreach (IBsonClassMapAttribute attribute in classMap.ClassType.GetCustomAttributes(typeof(IBsonClassMapAttribute), false))
                {
                    attribute.Apply(classMap);
                }
#pragma warning disable 618 // obsoleted by IBsonClassMapModifier
                foreach (IBsonClassMapModifier attribute in classMap.ClassType.GetCustomAttributes(typeof(IBsonClassMapModifier), false))
                {
                    // only apply this if it wasn't already applied
                    if (!(attribute is IBsonClassMapAttribute))
                    {
                        attribute.Apply(classMap);
                    }
                }
#pragma warning restore 618

                OptInMembersWithBsonMemberMapModifierAttribute(classMap);
                OptInMembersWithBsonCreatorMapModifierAttribute(classMap);
                IgnoreMembersWithBsonIgnoreAttribute(classMap);
                ThrowForDuplicateMemberMapAttributes(classMap);
            }

            public void Apply(BsonCreatorMap creatorMap)
            {
                if (creatorMap.MemberInfo != null)
                {
                    foreach (IBsonCreatorMapAttribute attribute in creatorMap.MemberInfo.GetCustomAttributes(typeof(IBsonCreatorMapAttribute), false))
                    {
                        attribute.Apply(creatorMap);
                    }
                }
            }

            public void Apply(BsonMemberMap memberMap)
            {
                foreach (IBsonMemberMapAttribute attribute in memberMap.MemberInfo.GetCustomAttributes(typeof(IBsonMemberMapAttribute), false))
                {
                    attribute.Apply(memberMap);
                }
#pragma warning disable 618 // obsoleted by IBsonMemberMapModifier
                foreach (IBsonMemberMapModifier attribute in memberMap.MemberInfo.GetCustomAttributes(typeof(IBsonMemberMapModifier), false))
                {
                    // only apply this if it wasn't already applied
                    if (!(attribute is IBsonMemberMapAttribute))
                    {
                        attribute.Apply(memberMap);
                    }
                }
#pragma warning restore 618
            }

            public void PostProcess(BsonClassMap classMap)
            {
                foreach (IBsonPostProcessingAttribute attribute in classMap.ClassType.GetCustomAttributes(typeof(IBsonPostProcessingAttribute), false))
                {
                    attribute.PostProcess(classMap);
                }
            }

            // private methods
            private bool AllowsDuplicate(Type type)
            {
                var usageAttribute = type.GetCustomAttributes(typeof(BsonMemberMapAttributeUsageAttribute), true)
                    .OfType<BsonMemberMapAttributeUsageAttribute>()
                    .SingleOrDefault();

                return usageAttribute == null || usageAttribute.AllowMultipleMembers;
            }

            private void OptInMembersWithBsonCreatorMapModifierAttribute(BsonClassMap classMap)
            {
                // let other constructors opt-in if they have any IBsonCreatorMapAttribute attributes
                foreach (var constructorInfo in classMap.ClassType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    var hasAttribute = constructorInfo.GetCustomAttributes(typeof(IBsonCreatorMapAttribute), false).Any();
                    if (hasAttribute)
                    {
                        classMap.MapConstructor(constructorInfo);
                    }
                }

                // let other static factory methods opt-in if they have any IBsonCreatorMapAttribute attributes
                foreach (var methodInfo in classMap.ClassType.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
                    var hasAttribute = methodInfo.GetCustomAttributes(typeof(IBsonCreatorMapAttribute), false).Any();
                    if (hasAttribute)
                    {
                        classMap.MapFactoryMethod(methodInfo);
                    }
                }
            }

            private void OptInMembersWithBsonMemberMapModifierAttribute(BsonClassMap classMap)
            {
                // let other fields opt-in if they have any IBsonMemberMapAttribute attributes
                foreach (var fieldInfo in classMap.ClassType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
#pragma warning disable 618 // obsoleted by IBsonMemberMapModifier
                    var hasAttribute = fieldInfo.GetCustomAttributes(typeof(IBsonMemberMapAttribute), false).Any()
                        || fieldInfo.GetCustomAttributes(typeof(IBsonMemberMapModifier), false).Any();
#pragma warning restore 618

                    if (hasAttribute)
                    {
                        classMap.MapMember(fieldInfo);
                    }
                }

                // let other properties opt-in if they have any IBsonMemberMapAttribute attributes
                foreach (var propertyInfo in classMap.ClassType.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                {
#pragma warning disable 618 // obsoleted by IBsonMemberMapModifier
                    var hasAttribute = propertyInfo.GetCustomAttributes(typeof(IBsonMemberMapAttribute), false).Any()
                        || propertyInfo.GetCustomAttributes(typeof(IBsonMemberMapModifier), false).Any();
#pragma warning restore 618
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
                    var ignoreAttribute = (BsonIgnoreAttribute)memberMap.MemberInfo.GetCustomAttributes(typeof(BsonIgnoreAttribute), false).FirstOrDefault();
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
                    var attributes = (IBsonMemberMapAttribute[])memberMap.MemberInfo.GetCustomAttributes(typeof(IBsonMemberMapAttribute), false);
#pragma warning disable 618 // obsoleted by IBsonMemberMapModifier
                    var legacyAttributes = (IBsonMemberMapModifier[])memberMap.MemberInfo.GetCustomAttributes(typeof(IBsonMemberMapModifier), false);
                    // combine them only if the modifier isn't already in the attributes list...
                    var attributeTypes = attributes
                        .Select(x => x.GetType())
                        .Union(legacyAttributes.Where(x => !(x is IBsonMemberMapAttribute)).Select(x => x.GetType()));
#pragma warning restore 618
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