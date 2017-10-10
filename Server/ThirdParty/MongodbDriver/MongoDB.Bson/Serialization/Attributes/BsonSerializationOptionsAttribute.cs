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
    public abstract class BsonSerializationOptionsAttribute : Attribute, IBsonMemberMapAttribute
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
            var serializer = memberMap.GetSerializer();
            var reconfiguredSerializer = Apply(serializer);
            memberMap.SetSerializer(reconfiguredSerializer);
        }

        // protected methods
        /// <summary>
        /// Reconfigures the specified serializer by applying this attribute to it.
        /// </summary>
        /// <param name="serializer">The serializer.</param>
        /// <returns>A reconfigured serializer.</returns>
        /// <exception cref="System.NotSupportedException"></exception>
        protected virtual IBsonSerializer Apply(IBsonSerializer serializer)
        {
            // if none of the overrides applied the attribute to the serializer see if it can be applied to a child serializer
            var childSerializerConfigurable = serializer as IChildSerializerConfigurable;
            if (childSerializerConfigurable != null)
            {
                var childSerializer = childSerializerConfigurable.ChildSerializer;
                var reconfiguredChildSerializer = Apply(childSerializer);
                return childSerializerConfigurable.WithChildSerializer(reconfiguredChildSerializer);
            }

            var message = string.Format(
                "A serializer of type '{0}' is not configurable using an attribute of type '{1}'.",
                BsonUtils.GetFriendlyTypeName(serializer.GetType()),
                BsonUtils.GetFriendlyTypeName(this.GetType()));
            throw new NotSupportedException(message);
        }
    }
}
