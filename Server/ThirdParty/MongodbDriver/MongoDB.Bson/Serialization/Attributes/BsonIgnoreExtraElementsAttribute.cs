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
    /// Specifies whether extra elements should be ignored when this class is deserialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class BsonIgnoreExtraElementsAttribute : Attribute, IBsonClassMapAttribute
    {
        // private fields
        private bool _ignoreExtraElements;
        private bool _inherited;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonIgnoreExtraElementsAttribute class.
        /// </summary>
        public BsonIgnoreExtraElementsAttribute()
            : this(true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BsonIgnoreExtraElementsAttribute class.
        /// </summary>
        /// <param name="ignoreExtraElements">Whether extra elements should be ignored when this class is deserialized.</param>
        public BsonIgnoreExtraElementsAttribute(bool ignoreExtraElements)
        {
            _ignoreExtraElements = ignoreExtraElements;
        }

        // public properties
        /// <summary>
        /// Gets whether extra elements should be ignored when this class is deserialized.
        /// </summary>
        public bool IgnoreExtraElements
        {
            get { return _ignoreExtraElements; }
        }

        /// <summary>
        /// Gets whether extra elements should also be ignored when any class derived from this one is deserialized.
        /// </summary>
        public bool Inherited
        {
            get { return _inherited; }
            set { _inherited = value; }
        }

        // public methods
        /// <summary>
        /// Applies a modification to the class map.
        /// </summary>
        /// <param name="classMap">The class map.</param>
        public void Apply(BsonClassMap classMap)
        {
            classMap.SetIgnoreExtraElements(_ignoreExtraElements);
            classMap.SetIgnoreExtraElementsIsInherited(_inherited);
        }
    }
}
