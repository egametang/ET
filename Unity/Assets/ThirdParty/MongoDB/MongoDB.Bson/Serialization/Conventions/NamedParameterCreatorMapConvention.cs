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
using System.Reflection;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// A convention that uses the names of the creator parameters to find the matching members.
    /// </summary>
    public class NamedParameterCreatorMapConvention : ConventionBase, ICreatorMapConvention
    {
        // public methods
        /// <summary>
        /// Applies a modification to the creator map.
        /// </summary>
        /// <param name="creatorMap">The creator map.</param>
        public void Apply(BsonCreatorMap creatorMap)
        {
            if (creatorMap.Arguments == null)
            {
                if (creatorMap.MemberInfo != null)
                {
                    var parameters = GetParameters(creatorMap.MemberInfo);
                    if (parameters != null)
                    {
                        var arguments = new List<MemberInfo>();

                        foreach (var parameter in parameters)
                        {
                            var argument = FindMatchingArgument(creatorMap.ClassMap.ClassType, parameter);
                            if (argument == null)
                            {
                                var message = string.Format("Unable to find a matching member to provide the value for parameter '{0}'.", parameter.Name);
                                throw new BsonException(message);
                            }
                            arguments.Add(argument);
                        }

                        creatorMap.SetArguments(arguments);
                    }
                }
            }
        }

        // private methods
        private MemberInfo FindMatchingArgument(Type classType, ParameterInfo parameter)
        {
            MemberInfo argument;
            if ((argument = Match(classType, MemberTypes.Property, BindingFlags.Public, parameter)) != null)
            {
                return argument;
            }
            if ((argument = Match(classType, MemberTypes.Field, BindingFlags.Public, parameter)) != null)
            {
                return argument;
            }
            if ((argument = Match(classType, MemberTypes.Property, BindingFlags.NonPublic, parameter)) != null)
            {
                return argument;
            }
            if ((argument = Match(classType, MemberTypes.Field, BindingFlags.NonPublic, parameter)) != null)
            {
                return argument;
            }
            return null;
        }

        private Type GetMemberType(MemberInfo memberInfo)
        {
            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null)
            {
                return fieldInfo.FieldType;
            }

            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null)
            {
                return propertyInfo.PropertyType;
            }

            // should never happen
            throw new BsonInternalException();
        }

        private IEnumerable<ParameterInfo> GetParameters(MemberInfo memberInfo)
        {
            var constructorInfo = memberInfo as ConstructorInfo;
            if (constructorInfo != null)
            {
                return constructorInfo.GetParameters();
            }

            var methodInfo = memberInfo as MethodInfo;
            if (methodInfo != null)
            {
                return methodInfo.GetParameters();
            }

            return null;
        }

        private MemberInfo Match(Type classType, MemberTypes memberType, BindingFlags visibility, ParameterInfo parameter)
        {
            var classTypeInfo = classType.GetTypeInfo();
            var bindingAttr = BindingFlags.IgnoreCase | BindingFlags.Instance;
            var memberInfos = classTypeInfo.GetMember(parameter.Name, memberType, bindingAttr | visibility);
            if (memberInfos.Length == 1 && GetMemberType(memberInfos[0]) == parameter.ParameterType)
            {
                return memberInfos[0];
            }
            return null;
        }
    }
}