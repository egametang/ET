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
    /// A convention that finds the extra elements member by name (and that is also of type BsonDocument or an IDictionary&lt;string, object&gt;).
    /// </summary>
#pragma warning disable 618 // about obsolete IExtraElementsMemberConvention
    public class NamedExtraElementsMemberConvention : ConventionBase, IClassMapConvention, IExtraElementsMemberConvention
#pragma warning restore 618
    {
        // private fields
        private readonly IEnumerable<string> _names;
        private readonly MemberTypes _memberTypes;
        private readonly BindingFlags _bindingFlags;

        // constructors
        /// <summary>
        /// Initializes a new instance of the NamedExtraElementsMemberConvention class.
        /// </summary>
        /// <param name="name">The name of the extra elements member.</param>
        public NamedExtraElementsMemberConvention(string name)
            : this(new [] { name })
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedExtraElementsMemberConvention" /> class.
        /// </summary>
        /// <param name="names">The names.</param>
        public NamedExtraElementsMemberConvention(IEnumerable<string> names)
            : this(names, BindingFlags.Instance | BindingFlags.Public)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedExtraElementsMemberConvention" /> class.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <param name="memberTypes">The member types.</param>
        public NamedExtraElementsMemberConvention(IEnumerable<string> names, MemberTypes memberTypes)
            : this(names, memberTypes, BindingFlags.Instance | BindingFlags.Public)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedExtraElementsMemberConvention" /> class.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        public NamedExtraElementsMemberConvention(IEnumerable<string> names, BindingFlags bindingFlags)
            : this(names, MemberTypes.Field | MemberTypes.Property, bindingFlags)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedExtraElementsMemberConvention" /> class.
        /// </summary>
        /// <param name="names">The names.</param>
        /// <param name="memberTypes">The member types.</param>
        /// <param name="bindingFlags">The binding flags.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public NamedExtraElementsMemberConvention(IEnumerable<string> names, MemberTypes memberTypes, BindingFlags bindingFlags)
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
        /// Gets the name of the convention.
        /// </summary>
        [Obsolete("There is no alternative.")]
        public new string Name
        {
            get { return _names.First(); }
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
                var member = classMap.ClassType.GetMember2(name, _memberTypes, _bindingFlags).SingleOrDefault();

                if (member != null)
                {
                    Type memberType = null;
                    var fieldInfo = member as FieldInfo;
                    if (fieldInfo != null)
                    {
                        memberType = fieldInfo.FieldType;
                    }
                    else
                    {
                        var propertyInfo = member as PropertyInfo;
                        if (propertyInfo != null)
                        {
                            memberType = propertyInfo.PropertyType;
                        }
                    }

                    if (memberType != null && (memberType == typeof(BsonDocument) || typeof(IDictionary<string, object>).IsAssignableFrom(memberType)))
                    {
                        classMap.MapExtraElementsMember(member);
                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Finds the extra elements member of a class.
        /// </summary>
        /// <param name="type">The class.</param>
        /// <returns>The extra elements member.</returns>
        [Obsolete("Use Apply instead.")]
        public string FindExtraElementsMember(Type type)
        {
            var memberInfo = type.GetMember(_names.First()).SingleOrDefault(x => x.MemberType == MemberTypes.Field || x.MemberType == MemberTypes.Property);
            return (memberInfo != null) ? _names.First() : null;
        }
    }
}