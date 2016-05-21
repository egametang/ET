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
using System.Reflection;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// Represents a BSON serialization options convention.
    /// </summary>
    [Obsolete("Use IMemberMapConvention instead.")]
    public interface ISerializationOptionsConvention
    {
        /// <summary>
        /// Gets the BSON serialization options for a member.
        /// </summary>
        /// <param name="memberInfo">The member.</param>
        /// <returns>The BSON serialization options for the member; or null to use defaults.</returns>
        IBsonSerializationOptions GetSerializationOptions(MemberInfo memberInfo);
    }

    /// <summary>
    /// Represents BSON serialiation options that use default values.
    /// </summary>
    [Obsolete("Use DelegateMemberMapConvention instead.")]
    public class NullSerializationOptionsConvention : ISerializationOptionsConvention
    {
        /// <summary>
        /// Gets the BSON serialization options for a member.
        /// </summary>
        /// <param name="memberInfo">The member.</param>
        /// <returns>
        /// The BSON serialization options for the member; or null to use defaults.
        /// </returns>
        public IBsonSerializationOptions GetSerializationOptions(MemberInfo memberInfo)
        {
            return null;
        }
    }

    /// <summary>
    /// Sets serialization options for a member of a given type.
    /// </summary>
    [Obsolete("Use MemberSerializationOptionsConvention instead.")]
    public class TypeRepresentationSerializationOptionsConvention : ISerializationOptionsConvention
    {
        private readonly Type _type;
        private readonly BsonType _representation;

        /// <summary>
        /// Initializes a new instance of the <see cref="TypeRepresentationSerializationOptionsConvention"/> class.
        /// </summary>
        /// <param name="type">The type of the member.</param>
        /// <param name="representation">The BSON representation to use for this type.</param>
        public TypeRepresentationSerializationOptionsConvention(Type type, BsonType representation)
        {
            _type = type;
            _representation = representation;
        }

        /// <summary>
        /// Gets the BSON serialization options for a member.
        /// </summary>
        /// <param name="memberInfo">The member.</param>
        /// <returns>
        /// The BSON serialization options for the member; or null to use defaults.
        /// </returns>
        public IBsonSerializationOptions GetSerializationOptions(MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            if (propertyInfo != null && propertyInfo.PropertyType == _type)
            {
                return new RepresentationSerializationOptions(_representation);
            }

            var fieldInfo = memberInfo as FieldInfo;
            if (fieldInfo != null && fieldInfo.FieldType == _type)
            {
                return new RepresentationSerializationOptions(_representation);
            }

            return null;
        }
    }

}