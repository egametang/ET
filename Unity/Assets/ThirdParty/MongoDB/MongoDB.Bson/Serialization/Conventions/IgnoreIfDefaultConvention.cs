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

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// A convention that sets whether to ignore default values during serialization.
    /// </summary>
    public class IgnoreIfDefaultConvention : ConventionBase, IMemberMapConvention
    {
        // private fields
        private bool _ignoreIfDefault;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreIfDefaultConvention" /> class.
        /// </summary>
        /// <param name="ignoreIfDefault">Whether to ignore default values during serialization.</param>
        public IgnoreIfDefaultConvention(bool ignoreIfDefault)
        {
            _ignoreIfDefault = ignoreIfDefault;
        }

        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// <param name="memberMap">The member map.</param>
        public void Apply(BsonMemberMap memberMap)
        {
            memberMap.SetIgnoreIfDefault(_ignoreIfDefault);
        }
    }
}
