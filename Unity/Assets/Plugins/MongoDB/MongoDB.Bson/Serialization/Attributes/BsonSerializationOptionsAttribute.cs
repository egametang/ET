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

namespace MongoDB.Bson.Serialization.Attributes
{
    /// <summary>
    /// Abstract base class for serialization options attributes.
    /// </summary>
#pragma warning disable 618 // obsoleted by IBsonMemberMapModifier
    public abstract class BsonSerializationOptionsAttribute : Attribute, IBsonMemberMapAttribute, IBsonMemberMapModifier
#pragma warning disable 618 
    {
        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonSerializationOptionsAttribute class.
        /// </summary>
        protected BsonSerializationOptionsAttribute()
        {
        }

        // public methods
        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// <param name="memberMap">The member map.</param>
        public virtual void Apply(BsonMemberMap memberMap)
        {
            var memberSerializer = memberMap.GetSerializer(memberMap.MemberType);
            var memberSerializationOptions = memberMap.SerializationOptions;
            if (memberSerializationOptions == null)
            {
                var memberDefaultSerializationOptions = memberSerializer.GetDefaultSerializationOptions();
                if (memberDefaultSerializationOptions == null)
                {
                    var message = string.Format(
                        "A serialization options attribute of type {0} cannot be used when the serializer is of type {1}.",
                        BsonUtils.GetFriendlyTypeName(this.GetType()),
                        BsonUtils.GetFriendlyTypeName(memberSerializer.GetType()));
                    throw new NotSupportedException(message);
                }
                memberSerializationOptions = memberDefaultSerializationOptions.Clone();
                memberMap.SetSerializationOptions(memberSerializationOptions);
            }
            memberSerializationOptions.ApplyAttribute(memberSerializer, this);
        }
    }
}
