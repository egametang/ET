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
    /// Specifies the default value for a field or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class BsonDefaultValueAttribute : Attribute, IBsonMemberMapAttribute
    {
        // private fields
        private object _defaultValue;
        private bool _serializeDefaultValue;
        private bool _serializeDefaultValueWasSet;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonDefaultValueAttribute class.
        /// </summary>
        /// <param name="defaultValue">The default value.</param>
        public BsonDefaultValueAttribute(object defaultValue)
        {
            _defaultValue = defaultValue;
        }

        // public properties
        /// <summary>
        /// Gets the default value.
        /// </summary>
        public object DefaultValue
        {
            get { return _defaultValue; }
        }

        /// <summary>
        /// Gets or sets whether to serialize the default value.
        /// </summary>
        [Obsolete("Use BsonIgnoreIfDefaultAttribute instead.")]
        public bool SerializeDefaultValue
        {
            get { return _serializeDefaultValue; }
            set {
                _serializeDefaultValue = value;
                _serializeDefaultValueWasSet = true;
            }
        }

        // public methods
        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// <param name="memberMap">The member map.</param>
        public void Apply(BsonMemberMap memberMap)
        {
            memberMap.SetDefaultValue(_defaultValue);
            if (_serializeDefaultValueWasSet)
            {
                memberMap.SetIgnoreIfNull(false);
                memberMap.SetIgnoreIfDefault(!_serializeDefaultValue);
            }
        }
    }
}
