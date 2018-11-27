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
using System.Reflection;
using MongoDB.Bson.Serialization.Options;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// A convention that allows you to set the Enum serialization representation
    /// </summary>
    public class EnumRepresentationConvention : ConventionBase, IMemberMapConvention
    {
        // private fields
        private readonly BsonType _representation;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumRepresentationConvention" /> class.
        /// </summary>  
        /// <param name="representation">The serialization representation. 0 is used to detect representation
        /// from the enum itself.</param>
        public EnumRepresentationConvention(BsonType representation)
        {
            if (!((representation == 0) ||
                (representation == BsonType.String) ||
                (representation == BsonType.Int32) ||
                (representation == BsonType.Int64)))
            {
                throw new ArgumentException("Enums can only be represented as String, Int32, Int64 or the type of the enum");
            }
            _representation = representation;
        }

        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// <param name="memberMap">The member map.</param>
        public void Apply(BsonMemberMap memberMap)
        {
            var memberTypeInfo = memberMap.MemberType.GetTypeInfo();
            if (memberTypeInfo.IsEnum)
            {
                var serializer = memberMap.GetSerializer();
                var representationConfigurableSerializer = serializer as IRepresentationConfigurable;
                if (representationConfigurableSerializer != null)
                {
                    var reconfiguredSerializer = representationConfigurableSerializer.WithRepresentation(_representation);
                    memberMap.SetSerializer(reconfiguredSerializer);
                }
            }
        }
    }
}
