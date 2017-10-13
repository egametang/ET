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
    /// Specifies the element name and related options for a field or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class BsonElementAttribute : Attribute, IBsonMemberMapAttribute
    {
        // private fields
        private string _elementName;
        private int _order = int.MaxValue;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonElementAttribute class.
        /// </summary>
        public BsonElementAttribute()
        { }

        /// <summary>
        /// Initializes a new instance of the BsonElementAttribute class.
        /// </summary>
        /// <param name="elementName">The name of the element.</param>
        public BsonElementAttribute(string elementName)
        {
            _elementName = elementName;
        }

        // public properties
        /// <summary>
        /// Gets the element name.
        /// </summary>
        public string ElementName
        {
            get { return _elementName; }
        }

        /// <summary>
        /// Gets the element serialization order.
        /// </summary>
        public int Order
        {
            get { return _order; }
            set { _order = value; }
        }

        // public methods
        /// <summary>
        /// Applies a modification to the member map.
        /// </summary>
        /// <param name="memberMap">The member map.</param>
        public void Apply(BsonMemberMap memberMap)
        {
            if (!string.IsNullOrEmpty(_elementName))
            {
                memberMap.SetElementName(_elementName);
            }
            memberMap.SetOrder(_order);
        }
    }
}