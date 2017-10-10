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

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// A convention that sets the default value for members of a given type.
    /// </summary>
    public class MemberDefaultValueConvention : ConventionBase, IMemberMapConvention
    {
        // private fields
        private readonly Type _type;
        private readonly object _defaultValue;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberDefaultValueConvention"/> class.
        /// </summary>
        /// <param name="type">The type of the member.</param>
        /// <param name="defaultValue">The default value for members of this type.</param>
        public MemberDefaultValueConvention(Type type, object defaultValue)
        {
            _type = type;
            _defaultValue = defaultValue;
        }

        // public methods
        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// <param name="memberMap">The member map.</param>
        public void Apply(BsonMemberMap memberMap)
        {
            if (memberMap.MemberType == _type)
            {
                memberMap.SetDefaultValue(_defaultValue);
            }
        }
    }
}