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
    /// A member map convention that wraps a delegate.
    /// </summary>
    public class DelegateMemberMapConvention : ConventionBase, IMemberMapConvention
    {
        // private fields
        private readonly Action<BsonMemberMap> _action;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="DelegateMemberMapConvention" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="action">The delegate.</param>
        public DelegateMemberMapConvention(string name, Action<BsonMemberMap> action)
            : base(name)
        {
            if (action == null)
            {
                throw new ArgumentNullException("action");
            }
            _action = action;
        }

        // public methods
        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// <param name="memberMap">The member map.</param>
        public void Apply(BsonMemberMap memberMap)
        {
            _action(memberMap);
        }
    }
}
