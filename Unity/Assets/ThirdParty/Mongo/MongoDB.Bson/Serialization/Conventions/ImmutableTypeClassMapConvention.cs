/* Copyright 2016-present MongoDB Inc.
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
* 
*/

using System;
using System.Linq;
using System.Reflection;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// Maps a fully immutable type. This will include anonymous types.
    /// </summary>
    public class ImmutableTypeClassMapConvention : ConventionBase, IClassMapConvention
    {
        /// <inheritdoc />
        public void Apply(BsonClassMap classMap)
        {
            var typeInfo = classMap.ClassType.GetTypeInfo();

            if (typeInfo.GetConstructor(Type.EmptyTypes) != null)
            {
                return;
            }

            var propertyBindingFlags = BindingFlags.Public | BindingFlags.Instance;
            var properties = typeInfo.GetProperties(propertyBindingFlags);
            if (properties.Any(CanWrite))
            {
                return; // a type that has any writable properties is not immutable
            }

            var anyConstructorsWereFound = false;
            var constructors = GetUsableConstructors(typeInfo);
            foreach (var ctor in constructors)
            {
                if (ctor.IsPrivate)
                {
                    continue; // do not consider private constructors
                }

                var parameters = ctor.GetParameters();

                var matches = parameters
                    .GroupJoin(properties,
                        parameter => parameter.Name,
                        property => property.Name,
                        (parameter, props) => new { Parameter = parameter, Properties = props },
                        StringComparer.OrdinalIgnoreCase);

                if (matches.Any(m => m.Properties.Count() != 1))
                {
                    continue;
                }

                if (ctor.IsPublic && !typeInfo.IsAbstract)
                {
                    // we need to save constructorInfo only for public constructors in non abstract classes
                    classMap.MapConstructor(ctor);
                }

                anyConstructorsWereFound = true;
            }

            if (anyConstructorsWereFound)
            {
                // if any constructors were found by this convention
                // then map all the properties from the ClassType inheritance level also
                foreach (var property in properties)
                {
                    if (property.DeclaringType != classMap.ClassType)
                    {
                        continue;
                    }
                    if (!PropertyMatchesSomeCreatorParameter(classMap, property))
                    {
                        continue;
                    }

                    var memberMap = classMap.MapMember(property);
                    if (classMap.IsAnonymous)
                    {
                        var defaultValue = memberMap.DefaultValue;
                        memberMap.SetDefaultValue(defaultValue);
                    }
                }
            }
        }

        // private methods
        private bool CanWrite(PropertyInfo propertyInfo)
        {
            // CanWrite gets true even if a property has only a private setter
            return propertyInfo.CanWrite && (propertyInfo.SetMethod?.IsPublic ?? false);
        }

        private ConstructorInfo[] GetUsableConstructors(TypeInfo typeInfo)
        {
            var constructorBindingFlags = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
            return typeInfo.GetConstructors(constructorBindingFlags);
        }

        private bool PropertyMatchesSomeCreatorParameter(BsonClassMap classMap, PropertyInfo propertyInfo)
        {
            foreach (var creatorMap in classMap.CreatorMaps)
            {
                if (creatorMap.MemberInfo is ConstructorInfo constructorInfo)
                {
                    if (PropertyMatchesSomeConstructorParameter(constructorInfo))
                    {
                        return true;
                    }
                }
            }

            // also map properties that match some constructor parameter that might be called by a derived class
            var classTypeInfo = classMap.ClassType.GetTypeInfo();
            var constructors = GetUsableConstructors(classTypeInfo);
            foreach (var constructorInfo in constructors)
            {
                if (classTypeInfo.IsAbstract || 
                    constructorInfo.IsFamily || // protected
                    constructorInfo.IsFamilyOrAssembly) // protected internal
                {
                    if (PropertyMatchesSomeConstructorParameter(constructorInfo))
                    {
                        return true;
                    }
                }
            }

            return false;

            bool PropertyMatchesSomeConstructorParameter(ConstructorInfo constructorInfo)
            {
                foreach (var parameter in constructorInfo.GetParameters())
                {
                    if (string.Equals(propertyInfo.Name, parameter.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}