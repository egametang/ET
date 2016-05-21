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

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// A convention that finds the id member by name.
    /// </summary>
#pragma warning disable 618 // about obsolete IIdMemberConvention
    public class NamedIdMemberConvention : ConventionBase, IClassMapConvention, IIdMemberConvention
#pragma warning restore 618
    {
        // private fields
        private readonly IEnumerable<string> _names;
        private readonly MemberTypes _memberTypes;
        private readonly BindingFlags _bindingFlags;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="NamedIdMemberConvention" /> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public NamedIdMemberConvention(params string[] names)
            : this((IEnumerable<string>)names)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedIdMemberConvention" /> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public NamedIdMemberConvention(IEnumerable<string> names)
            : this(names, BindingFlags.Instance | BindingFlags.Public)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedIdMemberConvention" /> class.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <param name="memberTypes">The member types.</param>
        public NamedIdMemberConvention(IEnumerable<string> names, MemberTypes memberTypes)
            : this(names, memberTypes, BindingFlags.Instance | BindingFlags.Public)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedIdMemberConvention" /> class.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        public NamedIdMemberConvention(IEnumerable<string> names, BindingFlags bindingFlags)
            : this(names, MemberTypes.Field | MemberTypes.Property, bindingFlags)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedIdMemberConvention" /> class.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <param name="memberTypes">The member types.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public NamedIdMemberConvention(IEnumerable<string> names, MemberTypes memberTypes, BindingFlags bindingFlags)
        {
            if (names == null)
            {
                throw new ArgumentNullException("names");
            }

            _names = names;
            _memberTypes = memberTypes;
            _bindingFlags = bindingFlags | BindingFlags.DeclaredOnly;
        }

        // public properties
        /// <summary>
        /// Gets the set of possible Id member names.
        /// </summary>
        [Obsolete("There is no alternative.")]
        public string[] Names
        {
            get { return _names.ToArray(); }
        }

        // public methods
        /// <summary>
        /// Applies a modification to the class map.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        public void Apply(BsonClassMap classMap)
        {
            foreach (var name in _names)
            {
				Type classType = classMap.ClassType;
	            var members = classType.GetMember2(name, _memberTypes, _bindingFlags);
				var member = members.SingleOrDefault();

				if (member != null)
                {
                    if (IsValidIdMember(classMap, member))
                    {
                        classMap.MapIdMember(member);
                        return;
                    }
                }
            }
        }

        private bool IsValidIdMember(BsonClassMap classMap, MemberInfo member)
        {
            if (member.MemberType == MemberTypes.Property)
            {
                var getMethodInfo = ((PropertyInfo)member).GetGetMethod(true);
                if (getMethodInfo.IsVirtual && getMethodInfo.GetBaseDefinition().DeclaringType != classMap.ClassType)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Finds the Id member of a class.
        /// </summary>
        /// <param name="type">The class.</param>
        /// <returns>The name of the Id member.</returns>
        [Obsolete("Use Apply instead.")]
        public string FindIdMember(Type type)
        {
            foreach (string name in _names)
            {
                var memberInfo = type.GetMember(name).SingleOrDefault(x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property);
                if (memberInfo != null)
                {
                    return name;
                }
            }
            return null;
        }
    }
}
