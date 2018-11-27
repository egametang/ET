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

using System.Reflection;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// A convention that finds readable and writeable members and adds them to the class map.
    /// </summary>
    public class ReadWriteMemberFinderConvention : ConventionBase, IClassMapConvention
    {
        // private fields
        private readonly BindingFlags _bindingFlags;
        private readonly MemberTypes _memberTypes;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadWriteMemberFinderConvention" /> class.
        /// </summary>
        public ReadWriteMemberFinderConvention()
            : this(BindingFlags.Instance | BindingFlags.Public)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadWriteMemberFinderConvention" /> class.
        /// </summary>
        /// <param name="memberTypes">The member types.</param>
        public ReadWriteMemberFinderConvention(MemberTypes memberTypes)
            : this(memberTypes, BindingFlags.Instance | BindingFlags.Public)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadWriteMemberFinderConvention" /> class.
        /// </summary>
        /// <param name="bindingFlags">The binding flags.</param>
        public ReadWriteMemberFinderConvention(BindingFlags bindingFlags)
            : this(MemberTypes.Field | MemberTypes.Property, bindingFlags)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadWriteMemberFinderConvention" /> class.
        /// </summary>
        /// <param name="memberTypes">The member types.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        public ReadWriteMemberFinderConvention(MemberTypes memberTypes, BindingFlags bindingFlags)
        {
            _memberTypes = memberTypes;
            _bindingFlags = bindingFlags | BindingFlags.DeclaredOnly;
        }

        // public methods
        /// <summary>
        /// Applies a modification to the class map.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        public void Apply(BsonClassMap classMap)
        {
            // order is important for backwards compatibility and GetMembers changes the order of finding things.
            // hence, we'll check member types explicitly instead of letting GetMembers handle it.

            if ((_memberTypes & MemberTypes.Field) == MemberTypes.Field)
            {
                var fields = classMap.ClassType.GetTypeInfo().GetFields(_bindingFlags);
                foreach (var field in fields)
                {
                    MapField(classMap, field);
                }
            }

            if ((_memberTypes & MemberTypes.Property) == MemberTypes.Property)
            {
                var properties = classMap.ClassType.GetTypeInfo().GetProperties(_bindingFlags);
                foreach (var property in properties)
                {
                    MapProperty(classMap, property);
                }
            }
        }

        // private methods
        private void MapField(BsonClassMap classMap, FieldInfo fieldInfo)
        {
            if (fieldInfo.IsInitOnly || fieldInfo.IsLiteral)
            {
                // we can't write
                return;
            }

            classMap.MapMember(fieldInfo);
        }

        private void MapProperty(BsonClassMap classMap, PropertyInfo propertyInfo)
        {
            if (!propertyInfo.CanRead || (!propertyInfo.CanWrite && classMap.ClassType.Namespace != null))
            {
                // we can't write or it is anonymous...
                return;
            }

            // skip indexers
            if (propertyInfo.GetIndexParameters().Length != 0)
            {
                return;
            }

            // skip overridden properties (they are already included by the base class)
            var getMethodInfo = propertyInfo.GetMethod;
            if (getMethodInfo.IsVirtual && getMethodInfo.GetBaseDefinition().DeclaringType != classMap.ClassType)
            {
                return;
            }

            classMap.MapMember(propertyInfo);
        }
    }
}