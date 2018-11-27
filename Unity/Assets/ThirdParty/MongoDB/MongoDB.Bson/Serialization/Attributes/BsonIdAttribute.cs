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
    /// Specifies that this is the Id field or property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    [BsonMemberMapAttributeUsage(AllowMultipleMembers = false)]
    public class BsonIdAttribute : Attribute, IBsonMemberMapAttribute
    {
        // private fields
        private Type _idGenerator;
        private int _order = int.MaxValue;

        // constructors
        /// <summary>
        /// Initializes a new instance of the BsonIdAttribute class.
        /// </summary>
        public BsonIdAttribute()
        {
        }

        // public properties
        /// <summary>
        /// Gets or sets the Id generator for the Id.
        /// </summary>
        public Type IdGenerator
        {
            get { return _idGenerator; }
            set { _idGenerator = value; }
        }

        /// <summary>
        /// Gets or sets the Id element serialization order.
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
            memberMap.SetOrder(_order);
            if (_idGenerator != null)
            {
                var idGenerator = (IIdGenerator)Activator.CreateInstance(_idGenerator); // public default constructor required
                memberMap.SetIdGenerator(idGenerator);
            }
            memberMap.ClassMap.SetIdMember(memberMap);
        }
    }
}
