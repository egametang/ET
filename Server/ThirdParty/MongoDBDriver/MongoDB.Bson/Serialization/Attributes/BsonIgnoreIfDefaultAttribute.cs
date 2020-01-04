﻿/* Copyright 2010-present MongoDB Inc.
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
    /// Indicates whether a field or property equal to the default value should be ignored when serializing this class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class BsonIgnoreIfDefaultAttribute : Attribute, IBsonMemberMapAttribute
    {
        // private fields
        private bool _value;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonIgnoreIfDefaultAttribute class.
        /// </summary>
        public BsonIgnoreIfDefaultAttribute()
        {
            _value = true;
        }

        /// <summary>
        /// Initializes a new instance of the BsonIgnoreIfDefaultAttribute class.
        /// </summary>
        /// <param name="value">Whether a field or property equal to the default value should be ignored when serializing this class.</param>
        public BsonIgnoreIfDefaultAttribute(bool value)
        {
            _value = value;
        }

        // public properties
        /// <summary>
        /// Gets whether a field or property equal to the default value should be ignored when serializing this class.
        /// </summary>
        public bool Value
        {
            get { return _value; }
        }

        // public methods
        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// <param name="memberMap">The member map.</param>
        public void Apply(BsonMemberMap memberMap)
        {
            memberMap.SetIgnoreIfNull(false);
            memberMap.SetIgnoreIfDefault(_value);
        }
    }
}
