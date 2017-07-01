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
using System.Reflection;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// Represents a member finder convention.
    /// </summary>
    [Obsolete("Use IClassMapConvention instead.")]
    public interface IMemberFinderConvention
    {
        /// <summary>
        /// Finds the members of a class that are serialized.
        /// </summary>
        /// <param name="type">The class.</param>
        /// <returns>The members that are serialized.</returns>
        IEnumerable<MemberInfo> FindMembers(Type type);
    }

    /// <summary>
    /// Represents a member finder convention where all public read/write fields and properties are serialized.
    /// </summary>
    [Obsolete("Use ReadWriteMemberFinderConvention instead.")]
    public class PublicMemberFinderConvention : IMemberFinderConvention
    {
        /// <summary>
        /// Finds the members of a class that are serialized.
        /// </summary>
        /// <param name="type">The class.</param>
        /// <returns>The members that are serialized.</returns>
        public IEnumerable<MemberInfo> FindMembers(Type type)
        {
            foreach (var fieldInfo in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (fieldInfo.IsInitOnly || fieldInfo.IsLiteral)
                {
                    // we can't write
                    continue;
                }

                yield return fieldInfo;
            }

            foreach (var propertyInfo in type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                if (!propertyInfo.CanRead || (!propertyInfo.CanWrite && type.Namespace != null))
                {
                    // we can't write or it is anonymous...
                    continue;
                }

                // skip indexers
                if (propertyInfo.GetIndexParameters().Length != 0)
                {
                    continue;
                }

                // skip overridden properties (they are already included by the base class)
                var getMethodInfo = propertyInfo.GetGetMethod(true);
                if (getMethodInfo.IsVirtual && getMethodInfo.GetBaseDefinition().DeclaringType != type)
                {
                    continue;
                }

                yield return propertyInfo;
            }
        }
    }
}