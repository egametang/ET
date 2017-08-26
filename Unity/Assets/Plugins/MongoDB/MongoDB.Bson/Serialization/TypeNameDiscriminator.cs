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
using System.Text.RegularExpressions;

namespace MongoDB.Bson.Serialization
{
    /// <summary>
    /// Supports using type names as discriminators.
    /// </summary>
    public static class TypeNameDiscriminator
    {
        // private static fields
        private static Assembly[] __wellKnownAssemblies;

        // static constructor
        static TypeNameDiscriminator()
        {
            __wellKnownAssemblies = new Assembly[]
            {
                typeof(object).GetTypeInfo().Assembly, // mscorlib
                typeof(Queue<>).GetTypeInfo().Assembly, // System
                typeof(HashSet<>).GetTypeInfo().Assembly // System.Core
            };
        }

        // public static methods
        /// <summary>
        /// Resolves a type name discriminator.
        /// </summary>
        /// <param name="typeName">The type name.</param>
        /// <returns>The type if type type name can be resolved; otherwise, null.</returns>
        public static Type GetActualType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
            {
                return type;
            }

            foreach (var assembly in __wellKnownAssemblies)
            {
                type = assembly.GetType(typeName);
                if (type != null)
                {
                    return type;
                }
            }

            string genericTypeDefinitionName;
            string[] typeArgumentNames;
            if (TryParseGenericTypeName(typeName, out genericTypeDefinitionName, out typeArgumentNames))
            {
                var genericTypeDefinition = GetActualType(genericTypeDefinitionName);
                if (genericTypeDefinition != null)
                {
                    var typeArguments = new List<Type>();
                    foreach (var typeArgumentName in typeArgumentNames)
                    {
                        var typeArgument = GetActualType(typeArgumentName);
                        if (typeArgument == null)
                        {
                            break;
                        }
                        typeArguments.Add(typeArgument);
                    }

                    if (typeArguments.Count == genericTypeDefinition.GetTypeInfo().GetGenericArguments().Length)
                    {
                        return genericTypeDefinition.MakeGenericType(typeArguments.ToArray());
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a type name to be used as a discriminator (like AssemblyQualifiedName but shortened for common DLLs).
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The type name.</returns>
        public static string GetDiscriminator(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }
            var typeInfo = type.GetTypeInfo();

            string typeName;
            if (typeInfo.IsGenericType)
            {
                var typeArgumentNames = "";
                foreach (var typeArgument in type.GetTypeInfo().GetGenericArguments())
                {
                    var typeArgumentName = GetDiscriminator(typeArgument);
                    if (typeArgumentName.IndexOf(',') != -1)
                    {
                        typeArgumentName = "[" + typeArgumentName + "]";
                    }
                    if (typeArgumentNames != "")
                    {
                        typeArgumentNames += ",";
                    }
                    typeArgumentNames += typeArgumentName;
                }
                typeName = type.GetGenericTypeDefinition().FullName + "[" + typeArgumentNames + "]";
            }
            else
            {
                typeName = type.FullName;
            }

            var assembly = type.GetTypeInfo().Assembly;
            string assemblyName = null;
            if (!__wellKnownAssemblies.Contains(assembly))
            {
                assemblyName = assembly.FullName;
                Match match = Regex.Match(assemblyName, "(?<dll>[^,]+), Version=[^,]+, Culture=[^,]+, PublicKeyToken=(?<token>[^,]+)");
                if (match.Success)
                {
                    var publicKeyToken = match.Groups["token"].Value;
                    if (publicKeyToken == "null")
                    {
                        var dllName = match.Groups["dll"].Value;
                        assemblyName = dllName;
                    }
                }
            }

            if (assemblyName == null)
            {
                return typeName;
            }
            else
            {
                return typeName + ", " + assemblyName;
            }
        }

        // private static methods
        private static bool TryParseGenericTypeName(string typeName, out string genericTypeDefinitionName, out string[] typeArgumentNames)
        {
            var leftBracketIndex = typeName.IndexOf('[');
            if (leftBracketIndex != -1)
            {
                genericTypeDefinitionName = typeName.Substring(0, leftBracketIndex);
                var typeArgumentNamesString = typeName.Substring(leftBracketIndex + 1, typeName.Length - leftBracketIndex - 2);
                var typeArgumentNamesList = new List<string>();
                var startIndex = 0;
                var nestingLevel = 0;
                for (var index = 0; index < typeArgumentNamesString.Length; index++)
                {
                    var c = typeArgumentNamesString[index];
                    switch (c)
                    {
                        case '[':
                            nestingLevel++;
                            break;
                        case ']':
                            nestingLevel--;
                            break;
                        case ',':
                            if (nestingLevel == 0)
                            {
                                var typeArgumentName = typeArgumentNamesString.Substring(startIndex, index - startIndex);
                                typeArgumentNamesList.Add(typeArgumentName);
                            }
                            break;
                    }
                }
                typeArgumentNamesList.Add(typeArgumentNamesString.Substring(startIndex));
                typeArgumentNames = typeArgumentNamesList.ToArray();
                return true;
            }

            genericTypeDefinitionName = null;
            typeArgumentNames = null;
            return false;
        }
    }
}
